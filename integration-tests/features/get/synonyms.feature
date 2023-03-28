Feature: A single best bet may have multiple synonyms

    Background:
        * url apiHost

    Scenario Outline: The same term should be found for all its synonyms
        for language '<lang>' and synonym '<synonym>'

        Given path 'BestBets', 'live', lang, synonym
        When method get
        Then status 200
        And match response == read( expected )
        Examples:
            | synonym           | lang | expected                         |
            | medullablastoma   | en   | synonyms-en-medulloblastoma.json |
            | medullablastoma   | en   | synonyms-en-medulloblastoma.json |
            | medulla blastoma  | en   | synonyms-en-medulloblastoma.json |
            | medulablastoma    | en   | synonyms-en-medulloblastoma.json |
            | medulloblastomas  | en   | synonyms-en-medulloblastoma.json |
            | higado            | es   | synonyms-es-higado.json          |
            | hepático          | es   | synonyms-es-higado.json          |
            | hepatico          | es   | synonyms-es-higado.json          |
            | hígado            | es   | synonyms-es-higado.json          |


    Scenario Outline: All items matching at least search token should be returned
        for language '<lang>' and synonymList '<synonymList>'

        Given path 'BestBets', 'live', lang, synonymList
        When method get
        Then status 200
        And match response == read(expectation)
        Examples:
            | synonymList        | lang | expectation                       |
            | visual DCIS        | en   | synonyms-en-multiple-matches.json |
            | fotos Medicamentos | es   | synonyms-es-multiple-matches.json |
