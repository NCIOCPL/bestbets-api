Feature: Best Bets returns an empty array when there is no match.

    Background:
        * url apiHost

    Scenario Outline: Status 200 and An empty array is returned when there are no matches
        for tokenList '<tokenList>', language: '<lang>'

        Given path 'bestbets', 'live', lang, tokenList
        When method get
        Then status 200
        And match response == []

        Examples:
            | tokenList             | lang |
            | chicken               | en   |
            | pollo                 | es   |
            | chicken snickerdoodle | en   |
            | pollo churro          | es   |
