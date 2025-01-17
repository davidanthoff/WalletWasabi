using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using ReactiveUI;
using WalletWasabi.Blockchain.TransactionBroadcasting;
using WalletWasabi.Blockchain.TransactionBuilding;
using WalletWasabi.CoinJoin.Client.Clients.Queuing;
using WalletWasabi.Fluent.Helpers;
using WalletWasabi.Fluent.Model;
using WalletWasabi.Fluent.ViewModels.Dialogs.Base;
using WalletWasabi.Fluent.ViewModels.Navigation;
using WalletWasabi.Wallets;

namespace WalletWasabi.Fluent.ViewModels.Wallets.Send
{
	[NavigationMetaData(Title = "Transaction Preview")]
	public partial class TransactionPreviewViewModel : RoutableViewModel
	{
		public TransactionPreviewViewModel(Wallet wallet, TransactionInfo info, TransactionBroadcaster broadcaster,
			BuildTransactionResult transaction)
		{
			var destinationAmount = transaction.CalculateDestinationAmount().ToDecimal(MoneyUnit.BTC);

			var fee = transaction.Fee;

			BtcAmountText = $"{destinationAmount} bitcoins ";

			FiatAmountText = $"(≈{(destinationAmount * wallet.Synchronizer.UsdExchangeRate).FormattedFiat()} USD) ";

			Labels = info.Labels.Labels.ToArray();

			AddressText = info.Address.ToString();

			ConfirmationTimeText = $"Approximately {TextHelpers.TimeSpanToFriendlyString(info.ConfirmationTimeSpan)} ";

			BtcFeeText = $"{fee.ToDecimal(MoneyUnit.Satoshi)} satoshis ";

			FiatFeeText =
				$"(≈{(fee.ToDecimal(MoneyUnit.BTC) * wallet.Synchronizer.UsdExchangeRate).FormattedFiat()} USD)";

			EnableCancel = true;

			EnableBack = true;

			NextCommand = ReactiveCommand.CreateFromTask(async () => await OnNext(wallet, broadcaster, transaction));
		}
		public string BtcAmountText { get; }

		public string FiatAmountText { get; }

		public string[] Labels { get; }

		public string AddressText { get; }

		public string ConfirmationTimeText { get; }

		public string BtcFeeText { get; }

		public string FiatFeeText { get; }

		private async Task OnNext(Wallet wallet, TransactionBroadcaster broadcaster, BuildTransactionResult transaction)
		{
			var transactionAuthorizationInfo = new TransactionAuthorizationInfo(transaction);
			var authDialog = AuthorizationHelpers.GetAuthorizationDialog(wallet, transactionAuthorizationInfo);
			var authDialogResult = await NavigateDialog(authDialog, authDialog.DefaultTarget);

			if (authDialogResult.Result)
			{
				IsBusy = true;

				// Dequeue any coin-joining coins.
				await wallet.ChaumianClient.DequeueAllCoinsFromMixAsync(DequeueReason.TransactionBuilding);

				await broadcaster.SendTransactionAsync(transactionAuthorizationInfo.Transaction);
				Navigate().Clear();

				IsBusy = false;
			}
			else if (authDialogResult.Kind == DialogResultKind.Normal)
			{
				await ShowErrorAsync("Authorization", "The Authorization has failed, please try again.", "");
			}
		}
	}
}
