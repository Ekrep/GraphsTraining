using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consoleable;
using TMPro;
using UnityEngine.UI;
using Consoleable.CommandTextObject;
using CollectionsTraining.CollectionUtils;
using System;
using Unity.VisualScripting;

public class CommandConsole : MonoBehaviour
{
    private static CommandConsole _instance;
    private List<IConsolable> consolables = new List<IConsolable>();
    [SerializeField] private List<Command> commandsBuffer = new List<Command>();
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform contentField;
    [SerializeField] private ConsoleTextObject messagePrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private bool isCurrentlyCommandWorking = false;
    private List<ConsoleTextObject> messages = new List<ConsoleTextObject>();

    private int complexityCountOfProcess;
    public static CommandConsole Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("Null Instance");
            }
            return _instance;
        }
    }
    public static event Action OnCommandComplete;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        _instance = this;
    }
    private void OnEnable()
    {
        InitializeTextField();
    }
    void OnDisable()
    {
        inputField.onEndEdit.RemoveAllListeners();
    }
    private void InitializeTextField()
    {
        inputField.onEndEdit.AddListener(SendCommandToListeners);
    }
    public void AssignCommandToBuffer(Command command)
    {
        commandsBuffer.Add(command);
    }

    public void AssignConsolable(IConsolable consolable)
    {
        consolables.Add(consolable);
    }

    public void SendCommandToListeners(string command)
    {
        if (command == "")
        {
            return;
        }
        HandleTextFieldOnMessageSended(command);
        if (command == "/help")
        {
            return;
        }
        BlockInputFieldUntilCommandDones();
        for (int i = 0; i < consolables.Count; i++)
        {
            consolables[i].SendCommand(command);
        }
    }
    private void BlockInputFieldUntilCommandDones()
    {
        inputField.text = "Pending...";
        inputField.interactable = false;
    }
    private void GetCommandablesCommands()
    {
        string commands = "";
        for (int i = 0; i < consolables.Count; i++)
        {
            commands += consolables[i].GetCommandList() + "\n";
        }
        var commandTextObj = Instantiate(messagePrefab);
        commandTextObj.transform.SetParent(contentField);
        commandTextObj.textMeshPro.text = commands;
    }
    public void RecieveResponse(string response, string name)
    {
        if (!isCurrentlyCommandWorking)
        {
            //Debug.Log("exe");
            //commandsBuffer[0].Execute();
            isCurrentlyCommandWorking = true;
        }
        var commandTextObj = Instantiate(messagePrefab);
        commandTextObj.transform.SetParent(contentField);
        commandTextObj.textMeshPro.text = $"<color=black><b>{name}-></b></color>" + "\n" + "<color=blue><b>Response:</b></color>" + " " + response + "\n" + "------------------------------------------";
        messages.Add(commandTextObj);
        StartCoroutine(AutoScroll());

    }
    private void HandleTextFieldOnMessageSended(string message)
    {
        if (message == "/help")
        {
            GetCommandablesCommands();
            inputField.text = "";
            StartCoroutine(AutoScroll());
            return;
        }
        inputField.text = "";
        var commandTextObj = Instantiate(messagePrefab);
        commandTextObj.transform.SetParent(contentField);
        commandTextObj.textMeshPro.text = "<color=green><b>Command:</b></color>" + " " + message;
        messages.Add(commandTextObj);
        StartCoroutine(AutoScroll());
    }
    private IEnumerator AutoScroll()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
        //decrease handle size(don't forget)

    }
    public void IncreaseComplexityCount()
    {
        complexityCountOfProcess++;
    }

    #region Event Functions
    public void CommandComplete()
    {
        OnCommandComplete?.Invoke();
        inputField.interactable = true;
        inputField.text = "";
        Debug.Log(complexityCountOfProcess);
        complexityCountOfProcess = 0;
        // commandsBuffer.RemoveAt(0);
        // if (commandsBuffer[0] != null)
        // {
        //     //commandsBuffer[0].Execute();
        // }
        // else
        // {
        //     //isCurrentlyCommandWorking = false;
        // }

    }

    #endregion
}
