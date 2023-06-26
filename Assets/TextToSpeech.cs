using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

/*
 * Autor: Nicolas Felipe Dipierro, 2022
*/
public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private List<AudioMixerGroup> genderMix = new List<AudioMixerGroup>(); //grupo de mixer para modificar el audio

    private string python_script_path;
    private string output_file_path;

    private AudioSource audioSource;
    [SerializeField] private AudioClip testClip;

    private int ID;
    private float volumeState;

    //generos disponibles
    public enum GENDER
    {
        MALE,
        FEMALE
    }

    //lenguajes soportados
    public enum LANG
    {
        es,
        en,
        fr,
    }

    private void Start()
    {
        ID = gameObject.GetInstanceID();
        UnityEngine.Debug.Log(ID);

        audioSource = gameObject.GetComponent<AudioSource>();

        //Estandariza el path del archivo que se va a ejecutar
        string filePath = Application.dataPath + "/Plugins/TTS/main.exe";
        python_script_path = filePath.Replace('/', '\\');

        //Estandariza el path donde se va a guardar el archivo de audio para que python lo entienda
        string outpath = Application.dataPath + "/Resources/TTS_Files";
        output_file_path = outpath.Replace('/', '\\');

        //UnityEngine.Debug.Log(output_file_path);

        volumeState = audioSource.volume;
    }

    [ContextMenu("Test Speak Spanish")]
    public void TestSpanish()
    {
        Speak("Aloja people", LANG.es, GENDER.MALE);
    }

    [ContextMenu("Test Speak English")]
    public void TestEnglish()
    {
        Speak("Hello world", LANG.en, GENDER.FEMALE);
    }

    [ContextMenu("Test Audio")]
    public void TestAudio()
    {
        audioSource.loop = true;
        audioSource.clip = testClip;
        audioSource.outputAudioMixerGroup = genderMix[0];
        audioSource.Play();
    }

    public void Speak(string msg, LANG lang, GENDER gender)
    {
        if (audioSource.loop) audioSource.loop = false;

        //Dependiendo el genero toma un mixer u otro
        if (gender == GENDER.MALE) audioSource.outputAudioMixerGroup = genderMix[1];
        else audioSource.outputAudioMixerGroup = genderMix[0];

        try
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.UseShellExecute = false;//utiliza la consola por defecto
                myProcess.StartInfo.FileName = python_script_path;//le pasa el path al ejecutable de python
                myProcess.StartInfo.CreateNoWindow = true;//no abre una ventana con la ejecucion

                //Argumentos necesario para que el ejecutable genere el audio
                //msg = mensaje a locutar
                //lang = lenguaje
                //output_file_path = path donde se va a guarda el audio
                //ID = id del objecto que va a utilizar el audio
                myProcess.StartInfo.Arguments = $"\"{msg}\" \"{lang}\" \"{output_file_path}\" \"{ID}\"";

                //Ejecuta el archivo de python
                myProcess.Start();

                string complete_file_path = output_file_path + $"\\output{ID}.mp3";

                StartCoroutine(UseAudioClip(complete_file_path));
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
        }
    }

    private IEnumerator UseAudioClip(string path)
    {


        yield return new WaitUntil(() => { return File.Exists(path); });

        UnityEngine.Debug.Log("<b>Finish Procces</b>");

        //Refresca los assets para que no haya problemas al abrir el archivo de audio
        AssetDatabase.Refresh();

        //cargo el archivo de audio generado particularmente para este objecto utilizando el ID
        AudioClip clip = Resources.Load<AudioClip>($"TTS_Files/output{ID}");

        UnityEngine.Debug.Log(clip.name);

        //lo ejecuta una vez
        audioSource.PlayOneShot(clip);

        StartCoroutine(DeleteAudioFile(path));
    }

    //Una vez finalizada la utilizacion del archivo de audio, se elimina para liberar espacio
    private IEnumerator DeleteAudioFile(string path)
    {
        yield return new WaitUntil(() => { return !audioSource.isPlaying; });

        if (File.Exists(path))
        {
            File.Delete(path);
            AssetDatabase.Refresh();
        }
    }

    public void MuteOffOn()
    {
        audioSource.volume = (audioSource.volume > 0) ? 0 : volumeState;
    }

    public void StopTTS()
    {
        audioSource.Stop();
    }
}
