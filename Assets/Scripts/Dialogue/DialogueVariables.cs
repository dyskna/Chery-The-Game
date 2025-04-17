using UnityEngine;
using Ink.Runtime;
using System.IO;
using System.Collections.Generic;


public class DialogueVariables
{
    private Dictionary<string, Ink.Runtime.Object> variables;
    
    public DialogueVariables(string globalsFilePath)
    {
        //compile
        string inkFileContents = File.ReadAllText(globalsFilePath);
        Ink.Compiler compiler = new Ink.Compiler(inkFileContents);
        Story globalVariablesStory = compiler.Compile();
        //initialize the dictionary
        variables = new Dictionary<string, Ink.Runtime.Object>();
        foreach (string name in globalVariablesStory.variablesState)
        {
            Ink.Runtime.Object value = globalVariablesStory.variablesState.GetVariableWithName(name);
            variables.Add(name, value);
        }

    }

    public void StartListening(Story story)
    {
        VariablesToStory(story);
        story.variablesState.variableChangedEvent += VariableChanged;
    }

    public void StopListening(Story story)
    {
        story.variablesState.variableChangedEvent -= VariableChanged;
    }

    private void VariableChanged(string name, Ink.Runtime.Object value)
    {
        //Debug.Log("Variable changed: " + name + "=" + value);
        if(variables.ContainsKey(name))
        {
            variables.Remove(name);
            variables.Add(name, value);
        }
    }

    private void VariablesToStory(Story story)
    {
        foreach(KeyValuePair<string, Ink.Runtime.Object> variable in variables)
        {
            story.variablesState.SetGlobal(variable.Key, variable.Value);
        }
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        variables.TryGetValue(variableName, out variableValue);
        if (variableValue == null)
        {
            Debug.LogWarning("Ink Variable was found to be null: "+ variableName);
        }
        return variableValue;
    }
}
