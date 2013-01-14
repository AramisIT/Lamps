namespace WMS_client.Base.Visual.Constructor
{
    public struct LabelForConstructor
    {
        public string Name { get; set; }
        public bool AllowEditValue { get; set; }
        public string Text { get; set; }
        public ControlsStyle Style { get; set; }
        public bool AddParameterData { get; set; }
        public int Skip { get; set; }

        public LabelForConstructor(string text)
            : this()
        {
            Text = text;
            Style = ControlsStyle.LabelSmall;
            AddParameterData = true;
        }

        public LabelForConstructor(string text, int skip)
            : this()
        {
            Text = text;
            Style = ControlsStyle.LabelSmall;
            AddParameterData = true;
            Skip = skip;
        }

        public LabelForConstructor(string text, bool addParameterData)
            : this()
        {
            Text = text;
            Style = ControlsStyle.LabelSmall;
            AddParameterData = addParameterData;
        }

        public LabelForConstructor(string text, ControlsStyle style)
            : this()
        {
            Text = text;
            Style = style;
            AddParameterData = false;
        }

        public LabelForConstructor(string text, ControlsStyle style, bool addParameterData):this()
        {
            Text = text;
            Style = style;
            AddParameterData = addParameterData;
        }

        public LabelForConstructor(string name, string text, ControlsStyle style, bool addParameterData)
            : this()
        {
            Name = name;
            Text = text;
            Style = style;
            AddParameterData = addParameterData;
        }

        public LabelForConstructor(string name, bool allowEdit, string text, ControlsStyle style, bool addParameterData)
            : this()
        {
            Name = name;
            AllowEditValue = allowEdit;
            Text = text;
            Style = style;
            AddParameterData = addParameterData;
        }
    }
}