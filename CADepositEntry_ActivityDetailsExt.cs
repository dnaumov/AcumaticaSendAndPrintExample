using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CR.DAC;
using PX.Objects.CR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;

using static SendEmailPrintReport.CADepositEntry_ActivityDetailsExt;

namespace SendEmailPrintReport
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CADepositEntry_ActivityDetailsExt : ActivityDetailsExt<CADepositEntry, DepositNotable, CADeposit.noteID>
	{
		[PXHidden]
		public class DepositNotable : CADeposit, INotable
		{ }

		public override Type GetBAccountIDCommand() => typeof(Select<Customer>);

		public override Type GetEmailMessageTarget() => typeof(Select<Contact, Where<Contact.contactID, Equal<Current<Customer.defBillContactID>>>>);

		public PXAction<CADeposit> EmailDeposit;
		[PXUIField(DisplayName = "Email", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable emailDeposit(PXAdapter adapter)
		{
			if (Base.Document.Current != null)
			{
				EmailDocument();
			}
			return adapter.Get();
		}

		public virtual void EmailDocument()
		{
			DepositNotable document = SelectFrom<DepositNotable>.Where<DepositNotable.refNbr.IsEqual<CADeposit.refNbr.FromCurrent>>.View.Select(Base);
			SendNotifications(
				sourceTypeGetter: _ => CRNotificationSource.BAccount,
				notifications: "PURCHASE ORDER",
				documents: new List<DepositNotable> { document },
				branchIDGetter: doc => doc.BranchID,
				documentParametersGetter: GetParameters,
				emailingParameters: new MassEmailingActionParameters()
			);
		}
		public static IDictionary<string, string> GetParameters(DepositNotable depositNotable)
		{
			return new Dictionary<string, string>();
		}
	}
}
