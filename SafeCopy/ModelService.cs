
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeCopy
{
    public class ModelService
    {
        public readonly UIDocument _uIDocument;
        public UIApplication _uiApplication;
        public Document doc;
        public Selection sel;
        public RevitEventHandler eventHandler;
        public XYZ DirectionPoint { get;private set; } = new XYZ();

        public ModelService(UIApplication app)
        {
            sel = app.ActiveUIDocument.Application.ActiveUIDocument.Selection;
            doc = app.ActiveUIDocument.Document;
        }
        public void SafeCopy(bool isLinear, int stepIn, int count)
        {
            List<ElementId> selectedElementsId = GetElementIdsFromSelect();
            if (isLinear) GetDirection(); else GetCenterPoint();   
            using (Transaction transaction = new Transaction(doc, "Безопасное копирование"))
            {
                transaction.Start();
                FailureHandlingOptions options = transaction.GetFailureHandlingOptions();
                MyPreProcessor preProcessor = new MyPreProcessor();
                options.SetFailuresPreprocessor(preProcessor);
                transaction.SetFailureHandlingOptions(options);
                if (isLinear)
                {
                    double directionVectorLenght = DirectionPoint.GetLength();
                    double cosX = DirectionPoint.X / directionVectorLenght, cosY = DirectionPoint.Y / directionVectorLenght, cosZ = DirectionPoint.Z / directionVectorLenght;
                    double step = ToFeet(stepIn);
                    for (int i = 1; i < count; i++)
                    {
                        double stepX = step * i * cosX, stepY = step * i * cosY, stepZ = step * i * cosZ;
                        XYZ translation = new XYZ(stepX, stepY, stepZ);
                        ElementTransformUtils.CopyElements(doc, selectedElementsId, translation);
                    }                    
                }
                else
                {
                    double stepInRadians = ToRadians(stepIn);
                    Line axis = Line.CreateBound(new XYZ(DirectionPoint.X, DirectionPoint.Y, DirectionPoint.Z), DirectionPoint.Add(XYZ.BasisZ ));
                    XYZ translation = new XYZ();
                    for(uint i = 1; i < count; i++)
                    {
                        double rotationAngle = stepInRadians * i;
                        ICollection<ElementId> newElemIds = ElementTransformUtils.CopyElements(doc, selectedElementsId, translation);
                        ElementTransformUtils.RotateElements(doc, newElemIds, axis, -rotationAngle);
                    }
                }
                transaction.Commit();

            }
        }

        public List<ElementId> GetElementIdsFromSelect()
        {
            List<ElementId> selectedElementsId = sel.GetElementIds().ToList();
            if (selectedElementsId.Count == 0)
            {
                var selectedReferences = sel.PickObjects(ObjectType.Element, "Выберите элементы для копирования");
                if (selectedReferences == null) throw new Exception("Не выбраны элементы для копирования");
                selectedElementsId = selectedReferences.Select(reference => (doc.GetElement(reference) as Element).Id).ToList();
            }
            return selectedElementsId;
        }
        public void GetCenterPoint()
        {
            DirectionPoint = sel.PickPoint("Выберите центральную точку");
        }
        public void GetDirection()
        {
            XYZ point0 = sel.PickPoint("Выберите первую точку направления");
            XYZ point1 = sel.PickPoint("Выберите вторую точку направления");
            DirectionPoint = new XYZ(point1.X - point0.X, point1.Y - point0.Y, point1.Z - point0.Z);
        }
        public static double ToFeet(double millimeters)
        {
            return UnitUtils.ConvertToInternalUnits(millimeters, UnitTypeId.Millimeters);
        }
        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
        public class MyPreProcessor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                string transactionName = failuresAccessor.GetTransactionName();
                IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
                if (fmas.Count == 0)
                {
                    return FailureProcessingResult.Continue;
                }
                if (transactionName.Equals("Безопасное копирование"))
                {
                    foreach (FailureMessageAccessor fma in fmas)
                    {
                        failuresAccessor.DeleteWarning(fma);
                    }
                    return FailureProcessingResult.ProceedWithCommit;
                }
                return FailureProcessingResult.Continue;
            }
        }
    }
}
