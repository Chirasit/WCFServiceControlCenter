using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JigControlKanagata
/// </summary>
public class JigControlKanagata : JigControl
{
    public JigControlKanagata()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    protected override ItemCheckingResult OnJigControlChecking(ItemCheckingEventArgs e)
    {
        ItemCheckingResult result = new ItemCheckingResult();
        result.HasError = false;
        result.WarningMessage = " ";
        result.ErrorMessage = "";
        return result;
    }
}