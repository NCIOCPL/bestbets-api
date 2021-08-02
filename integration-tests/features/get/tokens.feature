Feature: Best bets differentiates between similar terms with additional tokens

    Background:
        * url apiHost

    Scenario Outline: Distinguish between similar items based on differing number of tokens
        for token list: '<tokenList>', language: '<lang>'

        Given path 'BestBets', 'live', lang, tokenList
        When method get
        Then status 200
        And match response == read( expected )

        Examples:
            | tokenList   | lang | expected                       |
            | hodgkin     | en   | tokens-en-single-hodgkin.json  |
            | non hodgkin | en   | tokens-en-two-non-hodgkin.json |
