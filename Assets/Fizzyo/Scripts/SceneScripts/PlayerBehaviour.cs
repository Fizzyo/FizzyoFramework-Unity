using Fizzyo;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour {

    //Speed to move character at
    public float speed = 0.04f;
    public float flyHeight = 3.0f;
    Vector3 destPos;
    BreathRecogniser br = new BreathRecogniser();
    // Use this for initialization
    void Start () {
        br.BreathStarted += Br_BreathStarted;
        br.BreathComplete += Br_BreathComplete;
        destPos = transform.position;
    }

    private void Br_BreathStarted(object sender)
    {
        br.MaxBreathLength = FizzyoFramework.Instance.Device.maxBreathCalibrated;
        br.MaxPressure = FizzyoFramework.Instance.Device.maxPressureCalibrated;
    }

    private void Br_BreathComplete(object sender, ExhalationCompleteEventArgs e)
    {
        Debug.LogFormat("Breath Complete.\n Results: Quality [{0}] : Percentage [{1}] : Breath Full [{2}] : Breath Count [{3}] ", e.BreathQuality, e.BreathPercentage, e.IsBreathFull, e.BreathCount);
    }

    // Update is called once per frame
    void Update () {
        //move the player forward
        float x = transform.position.x + speed;
        //set height of the player using the player breath intensity
        float y = FizzyoFramework.Instance.Device.Pressure() * flyHeight;
        //Device.Pressure() can return negative numbers if the player is breathing in. Clamp the player height to be above 0
        y = Mathf.Max(y, 0);

        br.AddSample(Time.deltaTime, FizzyoFramework.Instance.Device.Pressure());

        transform.position = new Vector3(x,y, 0);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Level Restarted");
            SceneManager.LoadScene("ExampleLevel");
        }
	}
}
