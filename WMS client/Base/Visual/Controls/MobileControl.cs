namespace WMS_client
{
    public abstract class MobileControl
    {
        #region Public fields
        public string Name
        {
            get
            {
                string str = GetName();
                return str;
            }
            set { }
        }
        #endregion

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        #region Abstract methods
        public abstract string GetName();
        public abstract object GetControl();
        public abstract void Show();
        public abstract void Hide();
        #endregion
    }
}
