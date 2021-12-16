using NubankSharp;
using System.Diagnostics;
using System.IO;

namespace NubankSharp.Entities
{
    [DebuggerDisplay("Card: {GetCardName()}")]
    public class Card
    {
        public const string NUCONTA_NAME = "nuconta";
        public const string CREDIT_CARD_NAME = "credit-card";
        public const string CREDIT_CARD_BY_MONTH_NAME = "credit-card-by-month";

        public string UserName { get; }
        public string Name => GetCardName();
        public int BankId => Constants.BANK_ID;
        public int Agency { get; set; }
        public int Account { get; set; }
        public CardType CardType { get; set; }
        public StatementType StatementType { get; set; }

        public Card(string userName, CardType cardType, StatementType statementType, int agency = 0, int account = 0)
        {
            UserName = userName;
            StatementType = statementType;
            Agency = agency;
            Account = account;
            CardType = cardType;
        }

        private string GetCardName()
        {
            string name;
            if (CardType == CardType.CreditCard)
            {
                name = CREDIT_CARD_NAME;

                if (StatementType == StatementType.ByMonth)
                    name = CREDIT_CARD_BY_MONTH_NAME;
            }
            else
            {
                name = NUCONTA_NAME;
            }


            return name;
        }
    }
}
