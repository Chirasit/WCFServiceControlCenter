using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JigControl
/// </summary>
public abstract class JigControl
{
    protected abstract ItemCheckingResult OnJigControlChecking(ItemCheckingEventArgs e);
    public ItemCheckingResult JigControlChecking(ItemCheckingEventArgs e)
    {
        return null;

    }
}