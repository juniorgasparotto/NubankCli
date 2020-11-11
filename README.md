# NubankCli

NubankCli é um aplicativo de console que importa as transações do cartão de crédito e da NuConta em forma de arquivos JSONs segregados por usuário. Além disso, ele prove alguns comandos simples que ajudam a visualizar/sumarizar as transações importadas via linha de comando.

# Inicio rápido

Para iniciar é muito rápido e você pode fazer isso em qualquer terminal. Todo o material é focado no bash, caso queria usar outro terminal será necessário adaptação.

1. Caso não tenha o `dotnet core` instalado, faça-o pelo link: https://dotnet.microsoft.com/download
2. Faça o clone do projeto no seu local preferido e defina o comando simplificador `nu`:

    ```bash
    # Baixa o código na sua pasta corrente
    git clone https://github.com/juniorgasparotto/NubankCli.git

    # Entra na pasta
    cd NubankCli

    # Define um alias para o arquivo ./nu no qual contém um simplificador da execução do .net
    alias nu=./nu

    # Ou defina de maneira global e permanente
    echo alias nu="`pwd`/nu" >> ~/.bashrc; source ~/.bashrc;
    ```

3. Faça o login na CLI usando suas credencias do aplicativo NuBank

    ```
    nu login [cpf] [senha]
    ```

    * As informações do seu login ficarão salvas na pasta `src/NubankCli/UsersData/[cpf]`. Aqui temos o arquivo `user-info` que contém o seu token atual e os links descobertos da sua conta.

    * Você permanecerá logado até que o token do NuBank expire, normalmente demora-se 7 dias.

4. Após isso, será necessário fazer a segunda parte da autenticação e se tudo ocorreu bem, você verá um QRCode no seu terminal. Utilize o seu aplicativo do nubank em seu celular para validar o QRCode, use o menu:

    ```
    Ícone de configurações > Perfil > Acesso pelo site
    ```

5. Após a validação pelo celular, digite `enter` para prosseguir

6. Se tudo ocorrer bem você já estará logado e agora será possível importar suas transações de crédito e débito

7. Para importar as transações do cartão de crédito use:

    ```bash
    # Importa tudo sem nenhum filtro
    nu import-credit

    # Importa com filtro de data de inicio apenas
    nu import-credit --start 2020-01-01

    # Importa com filtro de data fim apenas
    nu import-credit --end 2020-02-01

    # Importa com filtro de data de inicio e fim
    nu import-credit --start 2020-01-01 --end 2020-02-01
    ```

8. Para importar as transações do cartão de débito (nuconta) use:

    ```bash
    # Importa tudo sem nenhum filtro
    nu import-debit

    # Importa com filtro de data de inicio apenas
    nu import-debit --start 2020-01-01

    # Importa com filtro de data fim apenas
    nu import-debit --end 2020-02-01

    # Importa com filtro de data de inicio e fim
    nu import-debit --start 2020-01-01 --end 2020-02-01
    ```

9. Para visualizar os extratos do seu cartão de crédito que foram importados, utilize o comando:

    ```bash
    # Visualiza todos os extratos importados de acordo com as datas de abertura e fechamento cada boleto
    nu get statement creditcard

    # Visualiza todos os extratos importados de forma mensal (forma longa)
    nu get statement creditcard --by-month

    # Visualiza todos os extratos importados de forma mensal (forma curta)
    nu get statement creditcard -h
    ```

    * Os dados importados de cartão de crédito ficaram dentro da sua pasta de usuário nas sub-pastas: `src/NubankCli/UsersData/[cpf]/card-credit`

10. Para visualizar os extratos do seu cartão de débito que foram importados, utilize o comando:

    ```bash
    nu get statement nuconta
    ```

    * Os dados importados de cartão de débito ficaram dentro da sua pasta de usuário nas sub-pastas: `src/NubankCli/UsersData/[cpf]/nuconta`.

11. Para visualizar os extratos consolidados do cartão de débito e débito (nuconta), utilize o comando:

    ```bash
    # Exibe extratos do cartão de crédito e débito
    nu get statement

    # Exibe extratos consolidando ambos os cartões de forma mensal (forma longa)
    nu get statement --merge

    # Exibe extratos consolidando ambos os cartões de forma mensal (forma curta)
    nu get statement -m 
    ```

12. Para visualizar todas as transações importadas, utilize o comando:

    ```bash
    # Simplificada
    nu get trans

    # Singular
    nu get transaction

    # Plural
    nu get transactions

    # Para visualizar mais colunas
    nu get transactions -o wide

    # Visualizar transações que iniciam com "IOF":
    nu get transactions "IOF"

    # Visualizar transações que tenham o ID:
    nu get transactions "5f52f4f6"
    ```

13. Para verificar quem está logado + informações do usuário:

    ```bash
    nu whoami
    ```

14. Para deslogar utilize o comando abaixo ou apague o arquivo `src/NubankCli/UsersData/[cpf]/user-info.json`:

    ```bash
    nu logout
    ```

# Outros comandos

Para importar os dados de cartão de crédito de forma mensal:

```bash
nu import-credit --statement-type ByBill 
```

Obtém apenas os extratos no qual contém alguma entrada:

```bash
nu get stat --where 'Transactions.Where(t => t.Value > 0).Sum(t => t.Value) > 0'
```

Ordena os extratos por valor de entrada do maior para o menor:

```bash
nu get stat --sort 'Transactions.Where(t => t.Value > 0).Sum(t => t.Value) DESC'
```

Comandos avançados para filtragem das transações importadas:

```bash
# Obtem as transações com filtro (forma curta -w): Apenas transações de entrada de valor (recebimentos)
nu get trans -w "Value > 0"

# Obtem as transações com filtro (forma longa --where): Apenas transações de saída de valor (pagamentos)
nu get trans --where "Value < 0"

# Obtem as transações ordenas por data (forma curta -s): Menor para o maior (mais antigas primeiro)
nu get trans -s "EventDate ASC"

# Obtem as transações ordenas por data (forma longa --sort): Maior para o menor (mais recentes primeiro)
nu get trans --sort "EventDate DESC"

# Obtem as maiores transações de saída
nu get trans -w "Value < 0" --sort "Value ASC"

# Obtem as maiores transações de entrada
nu get trans -w "Value > 0" --sort "Value DESC"

# Filtra pelo nome do cartão (nesse caso não obtem nada da NuConta)
nu get trans -w 'CardName="credit-card"'

# Obtem as transações de um extrato especifico (Start é a data de abertura da fatura - OpenDate)
nu get trans -w 'Statement.Start == "2020-05-17" && !IsBillPayment' -s "PostDate DESC, EventDate DESC"

# Obtem com uma quantidade mair de itens por página
nu get trans -S 100

# Obtem sem paginação
nu get trans -S 0

# Obtem em formato de JSON sem paginação removendo o cabeçalho e rodapé
nu get trans -o json -S 0 -v none
```

# Contribuíndo

Para contribuir basta ter instalado o `Visual Studio Code` ou o próprio `Visual Studio Community` e fazer as adaptações que ache necessárias. Vale dizer que o projeto é bem simples e não contém diversos recursos como:

* Geração de boleto
* Login com certificado
* Refresh Token

Fique a vontade para fazer essas evoluções ;)

## Wiremock

A pasta `Wiremock` contém algumas massas de dados que pode auxiliar na correção de bugs ou evoluções. Para usa-lo, basta instalar o wiremock e usar os arquivos desta pasta na execução do .jar do wiremock.

Os passos para executar usando o wiremock são:

1. Baixe o wiremock standalone: http://wiremock.org/docs/running-standalone/
2. Execute o wiremock apontando para a pasta onde temos a nossa massa de dados de testes:

    ```bash
    java -jar "C:\wiremock-standalone-2.27.2.jar" --port 6511 --root-dir "C:\NubankCli\wiremock" --match-headers Content-Type,Authorization,Accept
    ```
    
    * OBS: Estou considerando que todos os artefatos estejam na `C:`, troque para o caminho onde você baixou o `Wiremock` e o `NubankCli`.

3. Abra o arquivo `C:\NubankCli\src\NubankCli\settings.json` e altere a propriedade: `enableMockServer: true`
4. Por padrão, a porta `6511` já está configurada nesse arquivo na propriedade `mockUrl`, caso queira altera-la, mude o arquivo de configurações e execute o wiremock novamente na porta correta.

## Wiremock UI

Caso queria usar uma interface para o `Wiremock`, eu aconselho o `WiremockUI`, uma interface criada por mim que pode te ajudar a visualizar e manipular os servidores e arquivos do wiremock. Vale dizer que é uma interface exclusiva para Windows, para outros S.O é necessário usar o `.jar` diretamente.

https://github.com/juniorgasparotto/WiremockUI

## SysCommand

Esse projeto usa a biblioteca `SysCommand`, um parser de linha de comando para `.net` criado por mim que simplifica todo o trabalho de aplicações para console. O link abaixo contém todas as informações:

https://github.com/juniorgasparotto/SysCommand


## Agradecimentos

Esse projeto foi desenvolvido baseado na ideia de alguns outros repositórios no qual gostaria de fazer os devidos agradecimentos:

* https://github.com/lira92/nubank-dotnet
* https://github.com/andreroggeri/pynubank
* https://github.com/SpentBook/nubank-importer
