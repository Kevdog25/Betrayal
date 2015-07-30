using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ExposedComponent : MonoBehaviour {

    [SerializeField]
    private GameObject ComponentProperty;
    [SerializeField]
    private RectTransform ExpandedDescriptor;
    [SerializeField]
    private InputField NameField;

    private ScenarioComponent parentComponent;
    [System.NonSerialized]
    private EditorManager editor;
    public EditorManager Editor 
    {
        private get { return editor; }
        set { editor = value; }
    }

	// Use this for initialization
	void Start () {
	
	}

    /// <summary>
    /// Processes the component to update or set the displayed information
    /// as an arrangement of input fields that allowed to update/edit the component.
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="manager"></param>
    public void ProcessComponent(ScenarioComponent comp)
    {
        if (editor == null)
            throw new MissingReferenceException("EditorManager must be initialized first.");

        parentComponent = comp;

        // Put the name first, and handle it a little differently.
        NameField.text = comp.Name;
        NameField.onEndEdit.AddListener((x) => { comp.Name = x; });

        // Set up the input field for the string values.
        foreach (KeyValuePair<string, string> pair in comp.GetStringProperties())
        {
            var field = Instantiate(ComponentProperty);
            field.transform.SetParent(ExpandedDescriptor, false);
            var input = field.GetComponent<VariableInput>();
            input.Name.text = pair.Key;
            input.InputField.text = pair.Value;
            input.InputField.onEndEdit.AddListener
                (
                    (value) => { editor.SetComponentString(comp, input.Name.text, value); }
                );
        }

        // Set up the input field for the int values.
        foreach (KeyValuePair<string, int> pair in comp.GetIntProperties())
        {
            var field = Instantiate(ComponentProperty);
            field.transform.SetParent(ExpandedDescriptor, false);
            var input = field.GetComponent<VariableInput>();
            input.Name.text = pair.Key;
            input.InputField.text = pair.Value.ToString();
            input.InputField.onEndEdit.AddListener
                (
                    (value) => { editor.SetComponentInt(comp, input.Name.text, value); }
                );
        }

        // Set up the input field for the int values.
        foreach (KeyValuePair<string, float> pair in comp.GetFloatProperties())
        {
            var field = Instantiate(ComponentProperty);
            field.transform.SetParent(ExpandedDescriptor, false);
            var input = field.GetComponent<VariableInput>();
            input.Name.text = pair.Key;
            input.InputField.text = pair.Value.ToString();
            input.InputField.onEndEdit.AddListener
                (
                    (value) => { editor.SetComponentFloat(comp, input.Name.text, value); }
                );
        }
    }

    /// <summary>
    /// Calls the manager to remove the component that this is linked to.
    /// </summary>
    public void RemoveSelf()
    {
        editor.RemoveComponentFromScenario(parentComponent);
        DestroyImmediate(this.gameObject);
    }
}
