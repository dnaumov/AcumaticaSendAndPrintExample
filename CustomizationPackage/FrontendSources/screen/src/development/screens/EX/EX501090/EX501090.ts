
import { createCollection, createSingle, graphInfo, PXView, PXScreen, PXFieldState, gridConfig, PXFieldOptions, PXActionState, GridPreset } from "client-controls";

@graphInfo({
	graphType: "SendEmailPrintReport.PrintSendProcess",
	primaryView: "FilterView",
})
export class EX501090 extends PXScreen {
	FilterView = createSingle(FilterView);

	RecordsView = createCollection(DetailsViewClass);
}

export class FilterView extends PXView {
  ProcessingAction: PXFieldState;
}


@gridConfig({
	preset: GridPreset.Details
})
export class DetailsViewClass extends PXView {
  Selected: PXFieldState;
  CashAccountID: PXFieldState;
  Status: PXFieldState;

  RefNbr: PXFieldState;

  DocDate: PXFieldState;
}