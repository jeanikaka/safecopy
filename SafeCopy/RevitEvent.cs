using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SafeCopy
{
    public class RevitEventHandler :  IExternalEventHandler
    {
        private Action<UIApplication> _action;
        private  ExternalEvent _externalEvent;
        public RevitEventHandler()
        {
            _externalEvent = ExternalEvent.Create(this);
        }
        public void Run(Action<UIApplication> action)
        {
            _action = action;
            _externalEvent.Raise();

        }
        public void Execute(UIApplication app)
        {
            try
            {
                _action?.Invoke(app);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", ex.Message);
            }
        }

        private void Application_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public string GetName() => nameof(RevitEventHandler);
    }
}
