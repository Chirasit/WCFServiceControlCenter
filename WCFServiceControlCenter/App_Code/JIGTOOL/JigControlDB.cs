﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JigControlDB
/// </summary>
class JigControlDB : JigControl
{
    public JigControlDB()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    protected override ItemCheckingResult OnJigControlChecking(ItemCheckingEventArgs e)
    {
        ItemCheckingResult result = new ItemCheckingResult();
        result.HasError = true;
        result.WarningMessage = "WARNING I'm DB";
        return result;
    }
}