using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class SplashScreen : MonoBehaviour
{
    public string NextSceneName;
    public string LogoText;
    public Material PostProcessMat;
    public List<Sprite> SokSprites;
    public AudioClip Pop;
    public Image LogoImage;
    public Image LogoDropShadow;
    public Text LogoTextContainer;


    private AudioSource audioSource;
    private bool printText;
    private float angleRot;
    private float anglePlus;
    private float image_angle;
    private float logo_xscale;
    private float logo_yscale;
    private float scale;
    private float scaleTarg;
    private float letterIndex;
    private float previousLetterIndex;
    private float shadow_xscale;
    private float shadow_yscale;
    private int timer;
    private int drawTimer;
    private float overgangRadius;
    private float fakeTimer;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (((IEnumerable<string>)Environment.GetCommandLineArgs()).Select<string, string>((Func<string, string>)(s => s.ToLower())).Contains<string>("--no-intro"))
            SceneManager.LoadScene(this.NextSceneName);
        this.Draw();
        this.overgangRadius = 1.5f;
    }

    private void Start()
    {
        this.audioSource = this.GetComponent<AudioSource>();
        this.scaleTarg = 1f;
        this.anglePlus = 5f;
    }

    private void Update()
    {
        InputSystem.Update();
        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        SceneManager.LoadScene(this.NextSceneName);
    }

    private void FixedUpdate()
    {
        for (this.fakeTimer += Time.deltaTime; (double)this.fakeTimer >= 0.0166666675359011; this.fakeTimer -= 0.01666667f)
            this.FakeUpdate();
    }

    
    private void FakeUpdate()
    {
        this.Draw();
        ++this.drawTimer;
        if (this.drawTimer < 20)
            return;

        this.angleRot += (float)((double)this.anglePlus * 3.0 + 8.0);
        this.anglePlus *= 0.95f;
        this.angleRot %= 360f;
        this.image_angle = this.lengthdir_x(10f * this.anglePlus, this.angleRot) - 10f;
        this.scale += (float)(((double)this.scaleTarg - (double)this.scale) * 0.150000005960464);
        this.logo_xscale = (1f + this.lengthdir_x(0.1f * this.anglePlus, this.angleRot)) * this.scale;
        this.logo_yscale = (1f + this.lengthdir_y(0.1f * this.anglePlus, this.angleRot + 30f)) * this.scale;

        if ((double)this.anglePlus < 0.400000005960464 && !this.printText && audioSource != null)
        {
            //Should play sound here as well
            this.printText = true;
        }

        if (this.printText && (double)this.letterIndex < (double)LogoText.Length)
        {
            this.letterIndex += 0.15f;
            if (letterIndex >= previousLetterIndex + 1)
            {
                previousLetterIndex = Mathf.FloorToInt(letterIndex);
                this.audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                this.audioSource.PlayOneShot(Pop);
            }
        }


        this.shadow_xscale = (1f + this.lengthdir_x(0.11f * this.anglePlus, this.angleRot - 30f)) * this.scale;
        this.shadow_yscale = (1f + this.lengthdir_y(0.11f * this.anglePlus, this.angleRot - 10f)) * this.scale;
        
        ++this.timer;
        if (this.timer > 120)
        {
            this.overgangRadius -= 0.02f;
            //if ((double)this.overgangRadius < 0.0)
            //    SceneManager.LoadScene(this.NextSceneName);
        }
    }

    private void Draw()
    {
        this.LogoTextContainer.text = LogoText.Substring(0, Mathf.FloorToInt(this.letterIndex));
        this.LogoImage.transform.localScale = new Vector3(this.logo_xscale, this.logo_yscale, 1f);
        this.LogoImage.transform.localEulerAngles = new Vector3(0.0f, 0.0f, this.image_angle);
        this.LogoDropShadow.transform.localScale = new Vector3(this.shadow_xscale, this.shadow_yscale, 1f);
        this.LogoDropShadow.transform.localEulerAngles = new Vector3(0.0f, 0.0f, this.image_angle);
    }

    private float lengthdir_x(float len, float dir) => len * Mathf.Cos((float)Math.PI / 180f * dir);

    private float lengthdir_y(float len, float dir) => len * Mathf.Sin((float)Math.PI / 180f * dir);
}

