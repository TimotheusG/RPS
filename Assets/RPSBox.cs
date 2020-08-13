using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System;

enum Choices { None, Rock, Paper, Scissors}
public class RPSBox : NetworkBehaviour
{
    public GameObject prefabButton;
    [SyncVar]
    int playerNo;
    [SyncVar(hook = nameof(OnReadyChanged))]
    bool ready = false;
    [SyncVar(hook = nameof(OnWinnerChanged))]
    bool winner = false;
    [SyncVar(hook = nameof(OnLoserChanged))]
    bool loser = false;
    [SyncVar(hook = nameof(OnChoiceChanged))]
    Choices choice = 0;
    [SyncVar(hook = nameof(OnMessageChanged))]
    public string message;


    // This is called by the hook of playerData SyncVar above
    void OnMessageChanged(string oldPlayerData, string newPlayerData)
    {
        TextMeshProUGUI sb = GameObject.Find("ScoreBoard").GetComponent<TextMeshProUGUI>();
        sb.text = newPlayerData;
    }
    void OnChoiceChanged(Choices oldPlayerData, Choices newPlayerData)
    {
        if(choice != 0)
        {
            ready = true;
        }
    }
    void OnReadyChanged(bool oldPlayerData, bool newPlayerData)
    {
        CheckAllReady();
    }
    void OnWinnerChanged(bool oldPlayerData, bool newPlayerData)
    {
        message = "You win!";
    }
    void OnLoserChanged(bool oldPlayerData, bool newPlayerData)
    {
        message = "You lose!";
    }
    private void CheckAllReady()
    {
        List<RPSBox> readyPlayers = new List<RPSBox>();
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            RPSBox p = player.GetComponent<RPSBox>();
            if(p.ready)
            {
                readyPlayers.Add(p);
            }
        }
        //if all players are ready
        if(readyPlayers.Count == players.Length)
        {
            CompareChoices(readyPlayers);
            ResetValues(readyPlayers);
        }
    }

    private void ResetValues(List<RPSBox> readyPlayers)
    {
        foreach(var player in readyPlayers)
        {
            player.ready = false;
            player.choice = 0;
            player.winner = false;
        }
    }

    private void CompareChoices(List<RPSBox> readyPlayers)
    {
        var player1 = readyPlayers[0];
        var player2 = readyPlayers[1];
        //Rock & scissors
        if(player1.choice == Choices.Rock && player2.choice == Choices.Scissors)
        {
            player1.winner = true;
            player2.loser = true;
        }
        if (player1.choice == Choices.Scissors && player2.choice == Choices.Rock)
        {
            player2.winner = true;
            player1.loser = true;
        }
        //paper and scissors
        if (player1.choice == Choices.Paper && player2.choice == Choices.Scissors)
        {
            player2.winner = true;
            player1.loser = true;
        }
        if (player1.choice == Choices.Scissors && player2.choice == Choices.Paper)
        {
            player1.winner = true;
            player2.loser = true;
        }
        //rock and paper
        if (player1.choice == Choices.Paper && player2.choice == Choices.Rock)
        {
            player1.winner = true;
            player2.loser = true;
        }
        if (player1.choice == Choices.Rock && player2.choice == Choices.Paper)
        {
            player2.winner = true;
            player1.loser = true;
        }
        //draw
        if (player1.choice == player2.choice)
        {
            message = "Draw";
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        playerNo = connectionToClient.connectionId;
        message = "Pick!";
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //Add all options to the list
        List<string> options = new List<string>();
        foreach (var c in Enum.GetValues(typeof(Choices)))
        {
            //Remove None from the list
            var a = Enum.GetName(typeof(Choices), c);
            if(a != "None")
            {
                options.Add(a);
            }            
        }
        
        int i = 0;
        foreach (var option in options)
        {
            if (hasAuthority)
            {
                Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

                GameObject goButton = Instantiate(prefabButton);
                goButton.transform.SetParent(canvas.transform, false);
                goButton.transform.localScale = new Vector3(1, 1, 1);
                goButton.transform.localPosition = new Vector3(i * 150, 1, 1);
                TextMeshProUGUI text = goButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                text.text = option;
                Button tempButton = goButton.GetComponent<Button>();

                tempButton.onClick.AddListener(() => ButtonClicked(option));
                i++;
            }
        }
    }

    [Command]
    void ButtonClicked(string buttonName)
    {
        choice = (Choices)Enum.Parse(typeof(Choices), buttonName);
    }
}
