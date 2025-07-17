namespace Neptune.NsPay.CefTransferV2.Win.Classes
{
    public static class MyGlobal
    {
        public static void InvokeOnUiThreadIfRequired(this Control control, Action action)
        {
            if (control.Disposing || control.IsDisposed || !control.IsHandleCreated)
            {
                return;
            }

            if (control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }
    }
}
