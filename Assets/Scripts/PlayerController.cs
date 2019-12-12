using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public int counter = 0;
    public Animator anim;
    private Rigidbody2D playerRB;
    private ParticleSystem ps;
    public string player;
	public int next_command;
    public float moveto;
    public KeyCode up;
    public KeyCode down;
    public KeyCode power;
    public bool kick;
    public float speed;
    public float kick_threshold = .5f;
	public float hit_threshold = .25f;
    public float last_kick;
    public float kick_delta;
    private int guy_num;
    public float ypos;
    float enc_min = -25f;
    float enc_max = 25f;
    public Shader shader;
    private Matrix4x4 colorMatrix {
        get {
            Matrix4x4 mat = new Matrix4x4();
            mat.SetRow(0, new Vector4(0.16f, 0.16f, 0.16f, 1f));
            Color c = GameManager.gm.getColor(player);
            mat.SetRow(2, new Vector4(c.r, c.g, c.b, c.a));
            mat.SetRow(1, new Vector4(0.51f, 0.27f, 0f, 1f));
            mat.SetRow(3, new Vector4(1f, 0.76f, 0.56f, 1f));
            return mat;
        }
    }

    float[,] mapvals = new float[11, 2];

    // Use this for initialization
    void Start () {
        mapvals[0, 1] =  -2.5f;
        mapvals[0, 0] = -1.2f;
        mapvals[3, 1] =  -1.6f;
        mapvals[3, 0] = .8f; 
        mapvals[5, 1] =  -3.4f;
        mapvals[5, 0] = 0.4f; 
        mapvals[10, 1] =  -1.4f;
        mapvals[10, 0] = .8f; 

        anim = GetComponent<Animator>();
        anim.SetBool("kick", false);
        playerRB = GetComponent<Rigidbody2D>();
        ps = GetComponent<ParticleSystem>();
        if (player.Equals("p1")){
            guy_num = GameManager.gm.guys_nums;
            GameManager.gm.guys_nums++;
        } else {
            guy_num = -1;
        }
        ParticleSystem.EmissionModule emiss = ps.emission;
        emiss.enabled = false;

        setUpMaterial();
    }

    private void setUpMaterial() {
        Material mat = new Material(shader);
        mat.SetMatrix("_ColorMatrix", colorMatrix);
        GetComponent<SpriteRenderer>().material = mat;
    }

    public void Update() {
        if (player == "p1")
            anim.SetBool("kick", false);

		next_command = GameManager.gm.ncommand;

        if (next_command != -10101) {
            moveto = (float) next_command;
            if (guy_num == 0){
                ypos = moveto.Map(mapvals[guy_num, 0], mapvals[guy_num, 1], enc_min, enc_max);
                playerRB.position = new Vector2(playerRB.position.x, ypos);
            } else if (guy_num == 3){
                ypos = moveto.Map(mapvals[guy_num, 0], mapvals[guy_num, 1], enc_min, enc_max);
                playerRB.position = new Vector2(playerRB.position.x, ypos);
            }
            else if (guy_num == 5){
                ypos = moveto.Map(mapvals[guy_num, 0], mapvals[guy_num, 1], enc_min, enc_max);
                playerRB.position = new Vector2(playerRB.position.x, ypos);

            } else if (guy_num == 10){
                ypos = moveto.Map(mapvals[guy_num, 0], mapvals[guy_num, 1], enc_min, enc_max);
                playerRB.position = new Vector2(playerRB.position.x, ypos);
            }
        }
        if (GameManager.gm.hit) {
            anim.SetBool("kick", true);
            kick = true;
            last_kick = Time.time;
        } else {
            kick = false;
        }
        if (GameManager.gm.dir == -1) {
            playerRB.velocity = new Vector2(0, speed * .15f);
        }
        else if (GameManager.gm.dir == 1)
            playerRB.velocity = new Vector2(0, -speed * .15f);
        else playerRB.velocity = new Vector2(0, 0);
	}


    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
    }

    private void OnTriggerEnter2D (Collider2D collision) {
        if (collision.name == "ball") {
            if (player == "p1"){
                kick_delta = Time.time - last_kick;
				if (kick_delta < kick_threshold) {
					GameManager.gm.SendToArduino ("m -150 0\r");
					Rigidbody2D ballRB = collision.GetComponent<Rigidbody2D> ();
					float x = (1f + ballRB.velocity.x / 2.5f) + (playerRB.velocity.y / 4f);
					float y = (ballRB.velocity.y / 1.7f) + (playerRB.velocity.y / .5f);
					ballRB.velocity = new Vector2 (Math.Max (Math.Abs (x), 5.5f), y);
				} else {
					Rigidbody2D ballRB = collision.GetComponent<Rigidbody2D> ();
					float x = (-ballRB.velocity.x / 2.5f);
					float y = (-ballRB.velocity.y / 4f) + (playerRB.velocity.y / .5f);
                                        
                    string send_string = "m -150 0\r";

					//string send_string = "m 200 0\r";
					GameManager.gm.SendToArduino(send_string);
					ballRB.velocity = new Vector2 (Mathf.Clamp(x, -5.5f, 5.5f), y);
				}
            } else {
                anim.SetBool("kick", true);
                Rigidbody2D ballRB = collision.GetComponent<Rigidbody2D>();
                float x = (.5f + ballRB.velocity.x / 3.5f) + (playerRB.velocity.y / 4f);
                float y = (ballRB.velocity.y / 1.7f) + (playerRB.velocity.y / 1.1f);
                ballRB.velocity = new Vector2(Math.Min(-1 * Math.Abs(x), -5.5f), y);
            }
        }
        if (collision.name.Equals("p1_defenders_pin")){
            playerRB.velocity = new Vector2(0, 0);
        }
        if (collision.name.Equals("p1_midfield_pin")){
            playerRB.velocity = new Vector2(0, 0);
        }
        if (collision.name.Equals("p1_attack_pin")){
            playerRB.velocity = new Vector2(0, 0);
        }


    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name == "ball") {
            anim.SetBool("kick", false);
        }
    }
}
public static class f {
    public static float Map (this float value, float fromTarget, float toTarget, float fromSource, float toSource)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}