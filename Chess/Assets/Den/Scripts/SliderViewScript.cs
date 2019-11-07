using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderViewScript : MonoBehaviour
{
    public Text text;
    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener(SliderValueChanged);
        SliderValueChanged(slider.value);
    }
    void SliderValueChanged(float value)
    {
        text.text = ((int)(value *100)).ToString();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
