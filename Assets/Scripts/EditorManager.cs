using UnityEngine;
using System.Collections;
using System.IO;


public class EditorManager : MonoBehaviour {

	Scenario[] Scenarios;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Scenario[] RefreshScenarios(){
		var info = new DirectoryInfo(Path.Combine(Application.dataPath,"Scenarios/"));
		var fileInfo = info.GetFiles();
		Scenarios = new Scenario[fileInfo.Length];
		
		for(int i = 0; i < fileInfo.Length; i++){
			Debug.Log("Loading Scenario: " + fileInfo[i].FullName);
			Scenarios[i] = Scenario.Load(fileInfo[i].FullName);
		}

		return Scenarios;
	}
}
