using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class TOFPlayer
{
    public static Dictionary<ushort, TOFPlayer> players = new Dictionary<ushort, TOFPlayer>();
    public static Dictionary<ushort, TOFPlayer> enemy = new Dictionary<ushort, TOFPlayer>(); 

    public ushort id;
    public string username;
    
    public bool human;

    public Vector2Int position;
    public int rotation;
    
    public bool isAlive;
    public bool sinking;
    public int MaxHealth => maxHealth;
    
    public int CurrentHealth => currentHealth;
    
    int maxHealth;
    int currentHealth;

    public TOFPlayer(ushort id, Vector2Int startPosition, int startRotation, int maxHealth)
    {
        this.id = id;
        this.position = startPosition;
        this.rotation = startRotation;
        this.maxHealth = maxHealth;

        currentHealth = maxHealth;
        
        isAlive = true;
        sinking = false;

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.AddPlayerToList);

        message.AddUShort(id);
        message.AddVector2(startPosition);
        message.AddInt(startRotation);
        message.AddInt(currentHealth);
        message.AddInt(maxHealth);
        message.AddBool(isAlive);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void TakeDamage(int value)
    {
        currentHealth -= value;

        if (currentHealth <= 0)
        {
            isAlive = false;
            sinking = true;
        }

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.updateHealthValues);

        message.AddUShort(id);
        message.AddInt(CurrentHealth);
        message.AddBool(isAlive);
        message.AddBool(sinking);

        NetworkManager.Singleton.Server.SendToAll(message);
    }
}