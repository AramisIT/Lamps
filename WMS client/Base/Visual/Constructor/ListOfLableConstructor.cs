using System.Collections.Generic;

namespace WMS_client.Base.Visual.Constructor
{
    public class ListOfLabelsConstructor
    {
        private readonly WMSClient MainProcess;
        private readonly string Topic;
        private readonly object[] Parameters;
        
        public List<LabelForConstructor> ListOfLabels
        {
            get { return z_ListOfLabels; }
            set
            {
                z_ListOfLabels = value;
                refreshControls();
            }
        }
        private List<LabelForConstructor> z_ListOfLabels;

        public ListOfLabelsConstructor(WMSClient mainProcess)
        {
            MainProcess = mainProcess;
        }

        public ListOfLabelsConstructor(WMSClient mainProcess, object[] parameters)
        {
            MainProcess = mainProcess;
            Topic = string.Empty;
            Parameters = parameters;
        }

        public ListOfLabelsConstructor(WMSClient mainProcess, string topic, object[] parameters)
        {
            Topic = topic;
            Parameters = parameters;
            MainProcess = mainProcess;
        }

        private void refreshControls()
        {
            MainProcess.ClearControls();
            int top = 38;
            int index = 0;
            MainProcess.ToDoCommand = Topic ?? string.Empty;
            
            foreach (LabelForConstructor label in z_ListOfLabels)
            {
                int delta = label.Style == ControlsStyle.LabelH2 ||
                            label.Style == ControlsStyle.LabelH2Red
                                ? 23
                                : 18;
                top += delta;
                string text;

                if (label.AddParameterData)
                {
                    index += label.Skip;
                    string parameter = Parameters != null && Parameters.Length > index
                                           ? Parameters[index].ToString()
                                           : string.Empty;

                    text = string.Format(label.Text, parameter);
                    index++;
                }
                else
                {
                    text = label.Text;
                }

                MainProcess.CreateLabel(text, 5, top, 240, label.Style);
            }
        }
    }
}