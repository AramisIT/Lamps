//using NUnit.Framework;
//using WMS_client;
//using System;
//using System.Data.SqlServerCe;
//using WMS_client.db;

//namespace ClisentTest
//{
//    public static class TestHellper
//    {
//        public readonly static WMSClient Client;
//        public static readonly MainForm mForm;

//        static TestHellper()
//        {
//            mForm = new MainForm(true);
//            Client = new WMSClient(mForm);
//        }
//    }

//    public static class dbHellper
//    {
//        public static void PrepareElement<T>(string barcode) where T : dbObject, IBarcodeOwner
//        {
//            PrepareElement<T>(barcode, false);
//        }

//        public static void PrepareElement<T>(string barcode, bool clearAll)where T :dbObject, IBarcodeOwner
//        {
//            if(clearAll)
//            {
//                dbArchitector.ClearAll();
//            }
//            else
//            {
//                dbArchitector.ClearAllDataFromTable<T>();
//            }
            
//            dbObject newT = (dbObject)Activator.CreateInstance(typeof(T));
//            ((IBarcodeOwner) newT).BarCode = barcode;
//            newT.Save();
//        }
//    }

//    [TestFixture]
//    public class Processes_Test
//    {
//        [Test]
//        public void RegisterTest()
//        {
//            RegistrationProcess registration = new RegistrationProcess(TestHellper.Client);
//            registration.OnBarcode("9786175660690");

//            Type expectedNextType = typeof (SelectingLampProcess);
//            Type actualNextType = registration.MainProcess.Process.GetType();

//            Assert.AreEqual(expectedNextType, actualNextType);
//        }

//        [Test]
//        public void InstallNewLighter_NotFilled_Test()
//        {
//            InstallingNewLighter installing = new InstallingNewLighter(TestHellper.Client, "8000070018877");
//            installing.MainProcess.Process = null;
//            installing.Ok();

//            Assert.AreEqual(null, installing.MainProcess.Process);
//        }

//        [Test]
//        public void InstallNewLighter_IsFilled_Test()
//        {
//            InstallingNewLighter installing = new InstallingNewLighter(TestHellper.Client, "8000070018877")
//                                                  {
//                                                      MapInfo = new MapInfo(1, "Map", 1, 10),
//                                                      Register = "1",
//                                                      Position = "1"
//                                                  };
//            installing.Ok();

//            Type expectedNextType = typeof(FinishedInstalingNewLighter);
//            Type actualNextType = installing.MainProcess.Process.GetType();

//            Assert.AreEqual(expectedNextType, actualNextType);
//        }

//        [Test]
//        public void InstallNewLighter_CheckSave_Test()
//        {
//            object mapId = 1;
//            const string position = "1";
//            const string register = "2";
//            const string lighterBarcode = "8000070018878";

//            dbHellper.PrepareElement<Cases>(lighterBarcode);
//            FinishedInstalingNewLighter finished = new FinishedInstalingNewLighter(
//                TestHellper.Client,
//                new object[] {"Map", position, register},
//                mapId,
//                lighterBarcode);

//            finished.FinishedInstaling();

//            SqlCeCommand query =
//                dbWorker.NewQuery("SELECT Map,Register,Position FROM Cases WHERE RTRIM(Barcode)=RTRIM(@Barcode)");
//            query.AddParameter("Barcode", lighterBarcode);
//            object[] result = query.SelectArray();

//            dbArchitector.ClearAllDataFromTable<Cases>();
//            Assert.AreEqual(Convert.ToInt64(result[0]), Convert.ToInt64(mapId));
//            Assert.AreEqual(result[1].ToString(), position);
//            Assert.AreEqual(result[2].ToString(), register);
//        }
//    }
//}