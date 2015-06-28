using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DummyComponent : ScenarioComponent {

	// Use this for initialization
	public DummyComponent () {
		SetValues();
		SetTags();
	}

	void SetValues(){
		Name = "DummyComponent";
		StringValues.Add("key","value");
		IntValues.Add("key",1);
		FloatValues.Add("key",0.5f);
	}

	void SetTags(){
		Tags = new string[]
		{
			"Default",
			"Blank"
		};
	}

	public override string[] GetTags(){
		return Tags;
	}
}
