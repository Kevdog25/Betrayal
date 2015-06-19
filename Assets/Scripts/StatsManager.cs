using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatsManager : MonoBehaviour {
	public RectTransform HealthBar;
	public float MaxHealth;
	public Text box1;
	public Text box2;
	public Text box3;
	float MaxWidth;


	// Use this for initialization
	void Start () {
		MaxWidth = HealthBar.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetStats (Stats stats) {
		HealthBar.localScale = new Vector3(stats.Health * MaxWidth / MaxHealth,1,1);
		box1.text = stats.Speed.ToString();
		box2.text = stats.Strength.ToString();
		box3.text = stats.Intelligence.ToString();
	}
	





}
