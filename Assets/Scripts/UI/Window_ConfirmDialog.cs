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
		public Localizer Label_Message, Label_Title,
						 Label_Affirmative, Label_Negative;


		protected override void Awake()
		{
			base.Awake();
			Label_Affirmative.Key = "DIALOG_DEFAULT_OK";
			Label_Negative.Key = "DIALOG_DEFAULT_CANCEL";
		}

		public void Callback_FinishDialogue(bool okOrCancel)
		{
			Target(this, okOrCancel);
		}
	}
}
