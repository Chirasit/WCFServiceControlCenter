using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for ResultInfo
/// </summary>
[DataContract]
public class ResultInfo
{
    [DataMember]
    public bool HasError { get; set; }
    [DataMember]
    public string ErrorMessage { get; set; }
    [DataMember]
    public string WarningMessage { get; set; }


}