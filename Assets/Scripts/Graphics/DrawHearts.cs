﻿using UnityEngine;
using System.Collections;

public class DrawHearts : MonoBehaviour {
    public GameObject player;
    public Texture FullHeartTexture;
    public Texture EmptyHeartTexture;
    public Texture emptyStaminaTexture;
    public Texture staminaTexture;
    public Texture staminaGlowTexture;
    public float xOffset;
    public float yOffset;

    private PlayerStamina stamina;
    
	void Start () {
        stamina = player.GetComponent<PlayerStamina>();
	}
	
    void OnGUI() {
        Rect r;

        if (stamina.PotionActive) {
            r = new Rect(0, 2 * 30, 2 * 180, 2 * 32);
            GUI.DrawTexture(r, staminaGlowTexture);
        }

        // Draw hearts
        r = new Rect(xOffset, yOffset, Screen.width, Screen.height);
        GUILayout.BeginArea(r);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < stamina.maxHearts; ++i) {
            GUILayout.Label((i < stamina.Hearts) ? (FullHeartTexture) : (EmptyHeartTexture));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Draw stamina bar
        r = new Rect(5, 2 * 42, 2 * 160, 2 * 12);
        GUI.DrawTexture(r, emptyStaminaTexture);
        // Draw percentage bar
        r = new Rect(6, 2 * 43, 2 * 158 * stamina.Stamina / 100, 2 * 10);
        GUI.DrawTexture(r, staminaTexture);
    }
}
