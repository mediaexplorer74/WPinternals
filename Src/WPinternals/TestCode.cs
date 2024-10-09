// TestCode

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WPinternals
{
    internal static class TestCode
    {
        internal static async Task Test(System.Threading.SynchronizationContext UIContext)
        {
            // To avoid warnings when there is no code here.
            await Task.Run(() => { });

            // PhoneNotifierViewModel Notifier = new PhoneNotifierViewModel();
            // Notifier.Start();
            // await SwitchModeViewModel.SwitchTo(Notifier, PhoneInterfaces.Lumia_MassStorage);
            // MassStorage MassStorage = (MassStorage)Notifier.CurrentModel;
        }

        internal static async Task TestProgrammer(System.Threading.SynchronizationContext UIContext, 
            string ProgrammerPath)
        {
            Debug.WriteLine("[test] Begin action TestProgrammer...");
            LogFile.BeginAction("TestProgrammer");
            try
            {
                Debug.WriteLine("[test] Starting Firehose Test");
                LogFile.Log("Starting Firehose Test", LogType.FileAndConsole);

                PhoneNotifierViewModel Notifier = new PhoneNotifierViewModel();

                UIContext.Send(s => Notifier.Start(), null);

                if (Notifier.CurrentInterface == PhoneInterfaces.Qualcomm_Download)
                {
                    Debug.WriteLine("[test] Phone found in emergency mode.");
                    LogFile.Log("Phone found in emergency mode", LogType.FileAndConsole);
                }
                else
                {
                    Debug.WriteLine("[test] Phone needs to be switched to emergency mode.");
                    LogFile.Log("Phone needs to be switched to emergency mode.", LogType.FileAndConsole);

                    await SwitchModeViewModel.SwitchTo(Notifier, PhoneInterfaces.Lumia_Flash);

                    PhoneInfo Info = ((NokiaFlashModel)Notifier.CurrentModel).ReadPhoneInfo();

                    Debug.WriteLine("[test] Phone info:" + Info.ToString());
                    Info.Log(LogType.ConsoleOnly);

                    await SwitchModeViewModel.SwitchTo(Notifier, PhoneInterfaces.Qualcomm_Download);

                    if (Notifier.CurrentInterface != PhoneInterfaces.Qualcomm_Download)
                    {
                        Debug.WriteLine("[test] Switching mode failed.");
                        throw new WPinternalsException("Switching mode failed.");
                    }

                    Debug.WriteLine("[test] Phone is in emergency mode..");
                    LogFile.Log("Phone is in emergency mode.", LogType.FileAndConsole);
                }

                // Send and start programmer
                QualcommSerial Serial = (QualcommSerial)Notifier.CurrentModel;
                QualcommSahara Sahara = new QualcommSahara(Serial);

                if (await Sahara.Reset(ProgrammerPath))
                {
                    Debug.WriteLine("[test] Emergency programmer test succeeded.");
                    LogFile.Log("Emergency programmer test succeeded", LogType.FileAndConsole);
                }
                else
                {
                    Debug.WriteLine("[test] Emergency programmer test failed.");
                    LogFile.Log("Emergency programmer test failed", LogType.FileAndConsole);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] TestCode exception: " + ex.Message);
                LogFile.LogException(ex);
            }
            finally
            {
                Debug.WriteLine("[test] End action TestProgrammer.");
                LogFile.EndAction("TestProgrammer");
            }
        }
    }
}
