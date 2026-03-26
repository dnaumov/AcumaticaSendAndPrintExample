using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;

using System;
using System.Collections.Generic;

namespace SendEmailPrintReport
{
	public class PrintSendProcess : PXGraph<PrintSendProcess>
	{

		public PXCancel<PrintSendProcessFilter> Cancel;


		public PXFilter<PrintSendProcessFilter> FilterView;
		public SelectFrom<CADepositWithSelected>.ProcessingView.FilteredBy<PrintSendProcessFilter> RecordsView;

		[PXHidden]
		public class PrintSendProcessFilter : PXBqlTable, IBqlTable
		{
			#region ProcessingAction
			[PXString]
			[PXDefault("P")]
			[PXStringList(new string[] { "P", "E", "R" }, new string[] { "Print", "Email", "Release" })]
			[PXUIField(DisplayName = "Action")]
			public virtual string ProcessingAction { get; set; }
			public abstract class processingAction : PX.Data.BQL.BqlString.Field<processingAction> { }
			#endregion
		}
		[PXHidden]
		public class CADepositWithSelected: CADeposit
		{ 			
			#region Selected
			[PXBool]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected { get; set; }
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			#endregion
		}

		public PrintSendProcess()
		{
			RecordsView.Cache.AllowInsert = false;
			RecordsView.Cache.AllowDelete = false;

		}
		protected virtual void _(Events.RowSelecting<CADepositWithSelected> e)
		{
			if (e.Row != null && FilterView.Current?.ProcessingAction == "R"
				&& e.Row.Status == CADocStatus.Released)
			{
				e.Cancel = true;
			}
		}
		protected virtual void _(Events.RowSelected<PrintSendProcessFilter> e)
		{
			if (e.Row != null)
			{
				if (e.Row.ProcessingAction == "P")
				{
					RecordsView.SetProcessDelegate(PrintRecords);
				}
				else if (e.Row.ProcessingAction == "E")
				{
					RecordsView.SetProcessDelegate(EmailRecords);
				}
				else if (e.Row.ProcessingAction == "R")
				{
					RecordsView.SetProcessDelegate(ReleaseRecords);
				}
			}
		}

		public static void PrintRecords(List<CADepositWithSelected> list)
		{
			PXReportRequiredException reportRequiredException = null;
			foreach (var recordToPrint in list)
			{
				PXProcessing.SetCurrentItem(recordToPrint);
				Dictionary<string, string> parametersForReport = new Dictionary<string, string>();
				parametersForReport["TranType"] = recordToPrint.DocType;
				parametersForReport["RefNbr"] = recordToPrint.RefNbr;
				if (reportRequiredException == null)
				{
					reportRequiredException = PXReportRequiredException
						.CombineReport(reportRequiredException,
												  "CA656500", //report ID
												  parametersForReport);
				}
				else
				{
					reportRequiredException.AddSibling("CA656500", parametersForReport);
				}
				PXProcessing.SetProcessed();
			}
			throw reportRequiredException;

		}
		public static void EmailRecords(List<CADepositWithSelected> list)
		{
			var graph = PXGraph.CreateInstance<CADepositEntry>();
			foreach (var recordToPrint in list)
			{
				PXProcessing.SetCurrentItem(recordToPrint);
				graph.Clear();
				graph.Document.Current = graph.Document.Search<CADeposit.refNbr>(recordToPrint.RefNbr, recordToPrint.DocType);
				var depostitGraphExt = graph.GetExtension<CADepositEntry_ActivityDetailsExt>();
				depostitGraphExt.EmailDocument();
				PXProcessing.SetProcessed();
			}
		}
		public static void ReleaseRecords(List<CADepositWithSelected> list)
		{
			var graph = PXGraph.CreateInstance<CADepositEntry>();
			foreach (var recordToRelease in list)
			{
				PXProcessing.SetCurrentItem(recordToRelease);
				try
				{
					graph.Clear();
					graph.Document.Current = graph.Document.Search<CADeposit.refNbr>(recordToRelease.RefNbr, recordToRelease.DocType);
					if (graph.Document.Current == null)
					{
						throw new PXException("Deposit {0} of type {1} was not found.", recordToRelease.RefNbr, recordToRelease.DocType);
					}
					graph.release.Press();
					PXProcessing.SetProcessed();
				}
				catch (Exception e)
				{
					PXProcessing.SetError(e);
				}
			}
		}
	}
}