/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

Skyline Communications NV
Ambachtenstraat 33
B-8870 Izegem
Belgium
Tel.	: +32 51 31 35 69
Fax.	: +32 51 31 01 29
E-mail	: info@skyline.be
Web		: www.skyline.be
Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

11/01/2024	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace CoinMarketCap_1
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        private const string CoinMarketCapProtocolName = "Exercise HTTP CoinMarketCap";
        private const int ListingsUSDParamId = 8000;

        /// <summary>
        /// The Script entry point.
        /// IEngine.ShowUI();.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public static void Run(IEngine engine)
        {
            var app = new InteractiveController(engine);
            try
            {
                engine.SetFlag(RunTimeFlags.NoKeyCaching);
                engine.Timeout = TimeSpan.FromHours(10);

                RunSafe(engine, app);
            }
            catch (ScriptAbortException)
            {
                throw;
            }
            catch (ScriptForceAbortException)
            {
                throw;
            }
            catch (ScriptTimeoutException)
            {
                throw;
            }
            catch (InteractiveUserDetachedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                engine.Log($"Run|Something went wrong: {ex}");
                ShowExceptionDialog(engine, app, ex);
            }
        }

        private static void RunSafe(IEngine engine, InteractiveController app)
        {
            var dialog = new ExportDataDialog(engine);
            dialog.ExportDataButton.Pressed += (s, e) => ExportDataButton_Pressed(engine, app, dialog);
            app.Run(dialog);
        }

        private static void ExportDataButton_Pressed(IEngine engine, InteractiveController app, ExportDataDialog dialog)
        {
            var folderName = dialog.FolderTextBox.Text.Trim();
            var outputFolderPath = SecurePath.ConstructSecurePath("C:\\Skyline DataMiner\\Documents", folderName);
            Directory.CreateDirectory(outputFolderPath);

            var dms = engine.GetDms();
            var elements = dms.GetElements().Where(e => e.Protocol.Name == CoinMarketCapProtocolName);
            foreach (var element in elements)
            {
                var outputFilePath = SecurePath.ConstructSecurePath(outputFolderPath, $"{element.Name}.csv");
                File.WriteAllText(outputFilePath, GetCsvFromElement(element));
            }

            app.Stop();
        }

        private static string GetCsvFromElement(IDmsElement element)
        {
            var table = element.GetTable(ListingsUSDParamId);
            var data = table.GetData();
            var csvBuilder = new StringBuilder();

            var exportPIDs = new[] { 8001, 8002, 8003, 8004, 8005, 8006, 8007, 8008, 8009, 8010 };

            var columns = exportPIDs.Select(table.GetColumn<string>).ToArray();

            foreach (var column in columns)
            {
                csvBuilder.Append(column.Id);
                csvBuilder.Append(", ");
            }

            csvBuilder.AppendLine();

            foreach (var key in data.Keys)
            {
                foreach (var column in columns)
                {
                    csvBuilder.Append(column.GetValue(key, KeyType.PrimaryKey));
                    csvBuilder.Append(", ");
                }

                csvBuilder.AppendLine();
            }

            return csvBuilder.ToString();
        }

        private static void ShowExceptionDialog(IEngine engine, InteractiveController app, Exception exception)
        {
            ExceptionDialog exceptionDialog = new ExceptionDialog(engine, exception);
            exceptionDialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong.");
            if (app.IsRunning)
                app.ShowDialog(exceptionDialog);
            else
                app.Run(exceptionDialog);
        }

        public class ExportDataDialog : Dialog
        {
            public ExportDataDialog(IEngine engine) : base(engine)
            {
                var folderLabel = new Label("Output folder");
                FolderTextBox = new TextBox();
                ExportDataButton = new Button("Export");

                Title = "Choose folder to export data to";

                AddWidget(folderLabel, 0, 0, 1, 1);
                AddWidget(FolderTextBox, 0, 1, 1, 1);
                AddWidget(ExportDataButton, 2, 0, 1, 2);
            }

            public Button ExportDataButton { get; private set; }

            public TextBox FolderTextBox { get; private set; }
        }
    }
}