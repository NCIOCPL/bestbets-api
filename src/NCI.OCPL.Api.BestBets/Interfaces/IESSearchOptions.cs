namespace NCI.OCPL.Api.BestBets
{
    /// <summary>
    /// This interface contains the minimal information needed by an ElasticSearch backed
    /// service.
    /// </summary>
    public interface IESSearchOptions
    {
         /// <summary>
         /// Gets and sets a list of the ElasticSearch servers we will balance requests over 
         /// </summary>
         string[] Servers {get; set;}

         /// <summary>
         /// Gets and sets the Username for authenticating to the ES server
         /// </summary>
         string Username {get; set;}

         /// <summary>
         /// Gets and sets the Password for authenticating to the ES server
         /// </summary>
         string Password {get; set;}
    }
}