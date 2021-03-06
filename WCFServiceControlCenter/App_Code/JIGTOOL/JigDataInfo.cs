﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for JigDataInfo
/// </summary>
[DataContract]
public class JigDataInfo
{
    [DataMember]
    public int Id { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public string ShortName { get; set; }
    [DataMember]
    public string Type { get; set; }
    [DataMember]
    public string ExpireValue { get; set; }
    [DataMember]
    public string Status { get; set; }
    [DataMember]
    public bool IsChange { get; set; }
}