using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyUI
{
	/// <summary>
	/// A window that shows a dialog offering an affirmative and a negative response.
	/// Calls its Target function when a response is clicked.
	/// </summary>
	public class Window_ConfirmDialog : Window<Action<Window_ConfirmDialog, bool>>
	{
		public UnityEngine.UI.Text MessageLabel, TitleLabel,
								   AffirmativeLabel, NegativeLabel;


		public void Init(string title, string message,
						 string affirmative = null, string negative = null)
		{
            if (affirmative == null)
                affirmative = Localization.Get("DIALOG_DEFAULT_OK");
            if (negative == null)
                negative = Localization.Get("DIALOG_DEFAULT_CANCEL");

			MessageLabel.text = message;
			TitleLabel.text = title;
			AffirmativeLabel.text = affirmative;
			NegativeLabel.text = negative;
		}

		public void Callback_Click(bool result)
		{
			Target(this, result);
		}
	}
}
