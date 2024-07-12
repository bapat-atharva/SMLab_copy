using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneSwitch : MonoBehaviour
{
    public TMP_Dropdown sceneDropdown;

    void Start()
    {
        if (sceneDropdown != null)
        {
            sceneDropdown.onValueChanged.AddListener(DropdownValueChanged);
        }

        PopulateDropdown();

        //SetCurrentSceneAsSelected();

    }

    void PopulateDropdown()
    {
        // Clear existing options
        sceneDropdown.ClearOptions();

        // Create a list of scene names
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        options.Add(new TMP_Dropdown.OptionData("no scene"));
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            options.Add(new TMP_Dropdown.OptionData(sceneName));
        }

        // Add the options to the dropdown
        sceneDropdown.AddOptions(options);
    }

    void DropdownValueChanged(int index)
    {
        //Debug.Log(index);
        if (index > 0)
        SceneManager.LoadScene(index-1);
    }

    void SetCurrentSceneAsSelected()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Find the index of the current scene in the dropdown options
        int sceneIndex = sceneDropdown.options.FindIndex(option => option.text == currentSceneName);

        if (sceneIndex >= 0)
        {
            // Set the dropdown value to the index of the current scene
            sceneDropdown.value = sceneIndex;
            //sceneDropdown.RefreshShownValue();
        }
    }
}