using UnityEngine ;
using EasyUI.PickerWheelUI ;
using UnityEngine.UI ;
using System.Collections;

public class Demo : MonoBehaviour {
   [SerializeField] private Button uiSpinButton ;
   [SerializeField] private Text uiSpinButtonText ;

   [SerializeField] private PickerWheel pickerWheel ;

    private void Start()
    {

        uiSpinButton.onClick.AddListener(() =>
        {
            // Không cần disable button nữa, chỉ cần kiểm tra IsSpinning
            if (pickerWheel.IsSpinning) return;

            uiSpinButtonText.text = "Spinning";

            pickerWheel.OnSpinEnd(wheelPiece =>
            {
                Debug.Log(
                   @" <b>Index:</b> " + wheelPiece.Index + "           <b>Label:</b> " + wheelPiece.Label
                   + "\n <b>Amount:</b> " + wheelPiece.Amount + "      <b>Chance:</b> " + wheelPiece.Chance + "%"
                );

                // Delay một chút trước khi cho phép quay lại
                StartCoroutine(DelayedEnableButton());
            });

            pickerWheel.Spin();

        });

        /*   uiSpinButton.onClick.AddListener(() =>
           {

               uiSpinButton.interactable = false;
               uiSpinButtonText.text = "Spinning";

               pickerWheel.OnSpinEnd(wheelPiece =>
               {
                   Debug.Log(
                      @" <b>Index:</b> " + wheelPiece.Index + "           <b>Label:</b> " + wheelPiece.Label
                      + "\n <b>Amount:</b> " + wheelPiece.Amount + "      <b>Chance:</b> " + wheelPiece.Chance + "%"
                   );

                   uiSpinButton.interactable = true;
                   uiSpinButtonText.text = "Spin";
               });

               pickerWheel.Spin();

           });*/

    }


    // Thêm coroutine để delay
    private IEnumerator DelayedEnableButton()
    {
        uiSpinButtonText.text = "Spin";

        yield return new WaitForSeconds(2f); // Delay 2 giây

    }

}
