using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;

namespace NubankCli.Core.Repositories.Api
{
    public class NubankRepository
    {
        private const string StatementGraphQl = @"
        {
            viewer {
                savingsAccount {
                    id
                    feed {
                        id
                        __typename
                        title
                        detail
                        postDate
                        ... on BarcodePaymentFailureEvent {
                            amount
                        }
                        ... on TransferInEvent {
                            amount
                            originAccount {
                                name
                            }
                        }
                        ... on TransferOutEvent {
                            amount
                            destinationAccount {
                                name
                            }
                        }
                        ... on TransferOutReversalEvent {
                            amount
                        }
                        ... on BillPaymentEvent {
                            amount
                        }
                        ... on DebitPurchaseEvent {
                            amount
                        }
                        ... on BarcodePaymentEvent {
                            amount
                        }
                        ... on DebitWithdrawalFeeEvent {
                            amount
                        }
                        ... on DebitWithdrawalEvent {
                            amount
                        }    
                    }
                }
            }
        }";

        public RestSharpClient RestClient { get; set; }

        public string AuthToken { get; set; }
        public EndpointsRepository Endpoints { get; set; }

        public NubankRepository(RestSharpClient restClient, EndpointsRepository endpointsRepository)
        {
            RestClient = restClient;
            Endpoints = endpointsRepository;
        }

        public LoginResponse Login(string login, string password)
        {
            GetToken(login, password);

            if (Endpoints.Events != null)
                return new LoginResponse();

            return new LoginResponse(Guid.NewGuid().ToString());
        }

        private void GetToken(string login, string password)
        {
            var body = new
            {
                client_id = "other.conta",
                client_secret = "yQPeLzoHuJzlMMSAjC-LgNUJdUecx8XO",
                grant_type = "password",
                login,
                password
            };
            
            var response = RestClient.Post<Dictionary<string, object>>(Endpoints.Login, body);

            FillTokens(response);
            FillAutenticatedUrls(response);
        }

        public void AutenticateWithQrCode(string code)
        {
            var payload = new
            {
                qr_code_id = code,
                type = "login-webapp"
            };

            var response = RestClient.Post<Dictionary<string, object>>(Endpoints.Lift, payload, GetHeaders());

            FillTokens(response);
            FillAutenticatedUrls(response);
        }

        private void FillTokens(Dictionary<string, object> response)
        {
            if (!response.Keys.Any(x => x == "access_token"))
            {
                if (response.Keys.Any(x => x == "error"))
                {
                    throw new AuthenticationException(response["error"].ToString());
                }
                throw new AuthenticationException("Unknow error occurred on trying to do login on Nubank using the entered credentials");
            }
            AuthToken = response["access_token"].ToString();
        }

        private void FillAutenticatedUrls(Dictionary<string, object> response)
        {
            var listLinks = (JObject)response["_links"];
            var properties = listLinks.Properties();
            var values = listLinks.Values();
            Endpoints.AutenticatedUrls = listLinks
                .Properties()
                .Select(x => new KeyValuePair<string, string>(x.Name, (string)listLinks[x.Name]["href"]))
                .ToDictionary(key => key.Key, key => key.Value);
        }

        public IEnumerable<Saving> GetSavings()
        {
            EnsureAuthenticated();
            var response = RestClient.Post<GetSavingsResponse>(Endpoints.GraphQl,  new { query = StatementGraphQl.Trim() }, GetHeaders());
            return response.Savings;
        }

        public IEnumerable<Event> GetEvents()
        {
            EnsureAuthenticated();
            var response = RestClient.Get<GetEventsResponse>(Endpoints.Events, GetHeaders());
            return response.Events;
        }

        public IEnumerable<Bill> GetBills()
        {
            EnsureAuthenticated();
            var response = RestClient.Get<GetBillsResponse>(Endpoints.BillsSummary, GetHeaders());
            return response.Bills;
        }

        public Bill GetBill(string link)
        {
            EnsureAuthenticated();
            var response = RestClient.Get<GetBillResponse>(link, GetHeaders());
            return response.Bill;
        }


        private void EnsureAuthenticated()
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                throw new InvalidOperationException("This operation requires the user to be logged in. Make sure that the Login method has been called.");
            }
        }

        private Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string> {
                { "Authorization", $"Bearer {AuthToken}" }
            };
        }
    }
}