using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Projects;
using Managers;
using CivObjects;

class ProjectScrollView : MonoBehaviour
{ 
    public GameObject ProjectButton;    
    private GameData gameDataRef;
    private UIManager uiManagerRef;
    private List<GameObject> projectButtonList = new List<GameObject>();
    public bool listIsInitialized = false;

    void Awake()
    {
        gameDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
        uiManagerRef = GameObject.Find("GameManager").GetComponent<UIManager>();
    }

    void Update()
    {
       
    }
        

    public void InitializeList()
    {
            
        ClearList(); // clear out the list
        if (gameDataRef.CivList[0].Leader.ActionPoints > 0)
        {
            List<Project> projectList = new List<Project>();
            foreach (Project pData in gameDataRef.ProjectDataList)
            {
                if (pData.IsActionValid(uiManagerRef.ViewLevel))
                {
                    string type = pData.Type.ToString().ToUpper();
                    string level = uiManagerRef.PrimaryViewMode.ToString().ToUpper();

                    if (type == level)
                        AddProjectButton(pData);
                }
            }
        }
        listIsInitialized = true;         
    }

    private void AddProjectButton(Project pData)
    {
        GameObject go = Instantiate(ProjectButton) as GameObject;
        go.SetActive(true);
        go.name = pData.ID;
        ProjectButton PB = go.GetComponent<ProjectButton>();
        PB.SetName(pData.Name.ToUpper());
        PB.SetID(pData.ID);
        PB.SetIcon(pData.IconName);
        go.transform.SetParent(ProjectButton.transform.parent);
        go.transform.localScale = new Vector3(1, 1, 1); // to offset canvas scaling
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        projectButtonList.Add(go);
    }

    public void ClearList()
    {
        foreach (GameObject go in projectButtonList)
        {
            Destroy(go);
        }
    }

    //public void ButtonClicked(string str)
    //{
    //    Debug.Log(str + " button clicked.");
            
    //    // Action switch board
    //    uiManagerRef.ActivateProjectScreen(str); // launch the project screen
    //    listIsInitialized = false;
    //}
}
