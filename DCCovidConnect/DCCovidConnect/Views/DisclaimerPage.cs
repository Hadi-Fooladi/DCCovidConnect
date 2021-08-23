namespace DCCovidConnect.Views
{
    internal class DisclaimerPage : WebViewPage
    {
        public DisclaimerPage()
        {
            SetHtmlBody(BODY);
        }

        private const string BODY = @"
<p>The information contained in this application (“App”) is provided for informational purposes only and is not intended to be a substitute for personal judgement. <b>THIS APP DOES NOT OFFER MEDICAL ADVICE AND NOTHING CONTAINED HERIN IS INTENDED TO CONSTITUTE PROFESSIONAL ADVICE FOR A MEDICAL DIAGNOSIS OR TREATMENT. USING, ACCESSING OR BROWING THIS APP DOES NOT CREATE A REALTIONSHIP BETWEEN YOU AND CHILDREN’S NATIONAL HOSPITAL (OR ANY OF ITS AFFILIATED INSTITUTIONS).</b></p>

<p>Nothing contained in this App is intended to replace the medical advice or services of a licensed, trained physician or healthcare provider or to be a substitute for medical advice of a physician or healthcare professional licensed in your state. You should not rely on anything contained in this App or make any medical related decisions based in whole or in part of anything contained in the Information in this App. <b>INSTEAD YOU SHOULD USE SOUND JUDGEMENT AND CONSULT WITH A HEALTHCARE PROVIDER PROMPTLY REGARDING ANY COURSE OF TREATMENT, MEDICAL CONDITIONS, OR MEDICAL QUESTIONS YOU MAY HAVE.</b></p>

<p><b>THE USE OF THIS APP IS ENTIRELY AT YOUR OWN RISK,</b> and in no event shall Children’s National or any of its affiliates, officers, directors, employees and representatives be liable for any direct, indirect, incidental, consequential, special, exemplary, punitive, or any monetary or other damages, fees, fines, penalties, or liabilities arising out of or relating in any way to this App. Your sole and exclusive remedy for dissatisfaction with the App is to stop using it.</p>

<p>The information contained in this App may be changed at any times in Children’s National’s sole and absolute discretion without notice to you. The information is provided <b>“AS-IS.”</b> Children’s National does not guarantee or warrant against errors in the accuracy, timelines, and completeness of any Information contained in the App.</p>";
    }
}
