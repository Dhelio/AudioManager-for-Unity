using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe generica per la gestione della musica e degli effetti sonori
/// </summary>

namespace ReplaySRL {
    public class AudioManager : MonoBehaviour {

        //-------------------------------------------------------------------------------------------------------------------- VARIABILI PRIVATE
        private enum EmitterType { music, sfx, staticsfx } //tipo di AudioEmitter.

        private struct AudioEmitter {
            public EmitterType Type; //il tipo di AudioEmitter
            public AudioSource Source; //l'AudioSource di quell'emitter
        }

        private List<AudioEmitter> emittersList;
        private List<AudioEmitter> staticEmittersList;

        [SerializeField] private AudioClip[] musicClips;
        [SerializeField] private AudioClip[] sfxClips;
        [SerializeField] private AudioClip[] staticSfxClips;

        [SerializeField] private bool autoCreateListener = false, autoPlayMusic = false, dontDestroyOnLoad = false;

        //-------------------------------------------------------------------------------------------------------------------- VARIABILI PUBBLICHE
        public static AudioManager Instance; //Instanza singleton della classe

        //-------------------------------------------------------------------------------------------------------------------- FUNZIONI PRIVATE

        /// <summary>
        /// Sceglie casualmente una tra le tracce fornite ed aspetta che finisca. Quando finisce ne sceglie un'altra. Ad infinitum.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MusicPlayRandomEndless() {
            while (true) {
                int NUM = Random.Range(0, musicClips.Length);
                Debug.Log("-AudioManager- generato [" + NUM + "]");
                getMusicEmitter().Source.clip = musicClips[NUM];
                getMusicEmitter().Source.Play();
                yield return new WaitForSeconds(getMusicEmitter().Source.clip.length + .2f);
            }
        }

        /// <summary>
        /// Preleva, se esiste, l'AudioEmitter di tipo musica
        /// </summary>
        private AudioEmitter getMusicEmitter() {
            for (int i = 0; i < emittersList.Count - 1; i++) {
                if (emittersList[i].Type == EmitterType.music)
                    return emittersList[i];
            }
            emittersList.Add(new AudioEmitter { Type = EmitterType.music, Source = this.gameObject.AddComponent<AudioSource>() });
            return emittersList[emittersList.Count - 1];
        }

        /// <summary>
        /// Preleva un emitter di tipo sfx. Se non esiste lo crea.
        /// </summary>
        private AudioEmitter getSfxEmitter() {
            for (int i = 0; i < emittersList.Count - 1; i++) {
                if (emittersList[i].Type == EmitterType.sfx)
                    return emittersList[i];
            }
            emittersList.Add(new AudioEmitter { Type = EmitterType.sfx, Source = this.gameObject.AddComponent<AudioSource>() });
            return emittersList[emittersList.Count - 1];
        }

        /// <summary>
        /// Preleva un emitter del tipo fornito. Se non esiste lo crea.
        /// </summary>
        /// <param name="et">Tipo di emitter da ottenere</param>
        private AudioEmitter getEmitter(EmitterType et) {
            for (int i = 0; i < emittersList.Count - 1; i++) {
                if (emittersList[i].Type == et)
                    return emittersList[i];
            }
            emittersList.Add(new AudioEmitter { Type = et, Source = new AudioSource() });
            return emittersList[emittersList.Count - 1];
        }

        /// <summary>
        /// DEPRECATA. Controlla se l'emitter fornito è occupato. Se lo è, usa PlayClipAtPoint. Se non lo è allora esegue clip2play
        /// </summary>
        /// <param name="clip2play">Clip da eseguire</param>
        /// <param name="emitter">L'emitter da controllare</param>
        private bool CheckIsFree(AudioClip clip2play, AudioEmitter emitter) {
            if (emitter.Source.isPlaying) {
                AudioSource.PlayClipAtPoint(clip2play, this.gameObject.transform.position);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Trova un emitter di tipo SFX libero. Se non esiste lo crea.
        /// </summary>
        /// <returns>Un AudioEmitter libero.</returns>
        private AudioEmitter FindFreeSFXEmitter() {

            for (int i = 0; i < emittersList.Count - 1; i++) {
                if (emittersList[i].Type == EmitterType.sfx) {
                    if (!emittersList[i].Source.isPlaying)
                        return emittersList[i];
                }
            }

            AudioEmitter newEmitter = new AudioEmitter { Type = EmitterType.sfx, Source = this.gameObject.AddComponent<AudioSource>() };
            newEmitter.Source.volume = 1.0f;
            emittersList.Add(newEmitter);
            return emittersList[emittersList.Count - 1];
        }

        /// <summary>
        /// Abbassa gradualmente il volume dell'emitter musicale
        /// </summary>
        /// <param name="Time">In quanto tempo si deve azzerare il volume</param>
        private IEnumerator CoroutineFadeMusicOut(float Time) {
            AudioSource musicSource = getMusicEmitter().Source;
            float step = musicSource.volume / Time;
            while (musicSource.volume > 0) {
                musicSource.volume -= step;
                yield return new WaitForSeconds(.5f);
            }
        }

        /// <summary>
        /// Aspetta delay secondi e poi esegue clip sull'emitter musicale.
        /// </summary>
        /// <param name="delay">Quanto tempo deve aspettare</param>
        /// <param name="clip">La traccia da suonare</param>
        private IEnumerator PlayTrackDelayed(float delay, AudioClip clip) {
            yield return new WaitForSeconds(delay);
            getMusicEmitter().Source.clip = clip;
            getMusicEmitter().Source.Play();
        }

        /// <summary>
        /// Imposta i valori di default dell'audiosource
        /// </summary>
        /// <param name="x">Il source di cui impostare i valori</param>
        private void setDefaults(AudioSource x) {
            x.volume = 1;
            x.pitch = 1;
            x.clip = null;
        }

        /// <summary>
        /// Coroutine per il caricamento asincrono della musica
        /// </summary>
        private IEnumerator LoadMusicAsync(ResourceRequest req, bool autoplay) {
            while (!req.isDone) {
                yield return new WaitForSeconds(.25f);
            }
            if (autoplay) {
                PlayMusic();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------- FUNZIONI PUBBLICHE

        /// <summary>
        /// Imposta il loop del source della musica
        /// </summary>
        /// <param name="is_Looping">Se deve loopare o no</param>
        public void setMusicLoop(bool is_Looping) {
            getMusicEmitter().Source.loop = is_Looping;
        }

        /// <summary>
        /// Esegue la clip memorizzata in questo momento nel music emitter
        /// </summary>
        public void PlayMusic() {
            getMusicEmitter().Source.Play();
        }

        /// <summary>
        /// Esegue la traccia musicale dirattamente dall'ordine in cui è posta nel vettore
        /// </summary>
        /// <param name="id">Ordine nel vettore. Es. 0 farà eseguire la prima traccia, 1 la seconda, etc.</param>
        public void PlayMusicById(short id) {
            getMusicEmitter().Source.clip = musicClips[id];
            getMusicEmitter().Source.Play();
        }

        /// <summary>
        /// Esegue l'sfx prelevandolo dalla cartella Resource di Unity. Assicurarsi che sia quello il nome!
        /// </summary>
        public void PlayMusicByIdFromResource(string id) {
            getMusicEmitter().Source.clip = Resources.Load<AudioClip>(id);
            getMusicEmitter().Source.Play();
        }

        /// <summary>
        /// Carica istantaneamente la musica nel music player.
        /// </summary>
        /// <param name="id">Il nome che il l'audioclip ha nella cartella Resources.</param>
        public void LoadMusicByIdFromResource(string id) {
            getMusicEmitter().Source.clip = Resources.Load<AudioClip>(id);
        }

        /// <summary>
        /// Carica asincronamente la musica nel music player dalla cartella resource
        /// </summary>
        /// <param name="id">Il nome che il l'audioclip ha nella cartella Resources.</param>
        /// <param name="autoplay">Se deve essere eseguito o meno una volta che è stato caricato.</param>
        public void LoadAsyncMusicByIdFromResource(string id, bool autoplay) {
            ResourceRequest req = Resources.LoadAsync<AudioClip>(id);
            StartCoroutine(LoadMusicAsync(req, autoplay));
        }

        /// <summary>
        /// Esegue l'sfx prelevandolo dalla cartella Resource di Unity. Assicurarsi che sia quello il nome!
        /// </summary>
        public void PlayMusicByIdFromResource(string id, float volume) {
            getMusicEmitter().Source.clip = Resources.Load<AudioClip>(id);
            if (volume < 0.0f)
                getMusicEmitter().Source.volume = 1.0f;
            else
                getMusicEmitter().Source.volume = volume;
            getMusicEmitter().Source.Play();
        }

        /// <summary>
        /// Esegue la traccia musicale con il nome dato
        /// </summary>
        /// <param name="id">Il nome della traccia</param>
        public void PlayMusicById(string id) {
            for (int i = 0; i < musicClips.Length; i++) {
                if (id.Equals(musicClips[i].name)) {
                    getMusicEmitter().Source.Stop();
                    getMusicEmitter().Source.clip = musicClips[i];
                    getMusicEmitter().Source.Play();
                }
            }
        }

        /// <summary>
        /// Esegue l'sfx prelevandolo direttamente dall'ordine in cui è posto nel vettore.
        /// </summary>
        /// <param name="id">Ordine nel vettore. Es. 0 farà eseguire il primo sfx, 1 il secondo, etc.</param>
        public void PlaySfxById(short id) {
            AudioEmitter tmp = FindFreeSFXEmitter();
            setDefaults(tmp.Source);
            tmp.Source.clip = sfxClips[id];
            tmp.Source.Play();
        }

        /// <summary>
        /// Esegue l'sfx prelevandolo direttamente dall'ordine in cui è posto nel vettore, assegnandogli il volume passato.
        /// </summary>
        /// <param name="ID">Ordine nel vettore. Es.0 farà eseguire il primo sfx, 1 il secondo, etc.</param>
        /// <param name="Volume">Volume dell'effetto, da 0.0f a 1.0f</param>
        public void PlaySfxById(short ID, float Volume) {
            AudioEmitter tmp = FindFreeSFXEmitter();
            tmp.Source.clip = sfxClips[ID];
            tmp.Source.volume = Volume;
            tmp.Source.pitch = 1.0f;
            tmp.Source.Play();
        }

        /// <summary>
        /// /// Esegue l'sfx prelevandolo direttamente dall'ordine in cui è posto nel vettore, assegnandogli il volume ed il pitch passato.
        /// </summary>
        /// <param name="ID">Ordine nel vettore. Es.0 farà eseguire il primo sfx, 1 il secondo, etc.</param>
        /// <param name="Volume">Volume dell'effetto, da 0.0f a  1.0f</param>
        /// <param name="Pitch">Pitch dell'effetto</param>
        public void PlaySfxById(short ID, float Volume, float Pitch) {
            AudioEmitter tmp = FindFreeSFXEmitter();
            tmp.Source.clip = sfxClips[ID];
            tmp.Source.volume = Volume;
            tmp.Source.pitch = Pitch;
            tmp.Source.Play();
        }

        /// <summary>
        /// Esegue l'sfx con il nome dato.
        /// </summary>
        /// <param name="id">nome dell'sfx</param>
        public void PlaySfxById(string id) {
            for (int i = 0; i < sfxClips.Length; i++) {
                if (sfxClips[i].name == id) {
                    AudioEmitter tmp = FindFreeSFXEmitter();
                    setDefaults(tmp.Source);
                    tmp.Source.clip = sfxClips[i];
                    tmp.Source.Play();
                }
            }
        }

        /// <summary>
        /// Esegue l'SFX con l'ID dato
        /// </summary>
        /// <param name="id">Nome dell'SFX</param>
        /// <param name="volume">Volume dell'SFX</param>
        /// <param name="pitch">Pitch dell'SFX</param>
        public void PlaySfxById(string id, float volume, float pitch) {
            for (int i = 0; i < sfxClips.Length; i++) {
                if (sfxClips[i].name == id) {
                    AudioEmitter tmp = FindFreeSFXEmitter();
                    tmp.Source.clip = sfxClips[i];
                    tmp.Source.volume = volume;
                    tmp.Source.pitch = pitch;
                    tmp.Source.Play();
                }
            }
        }

        /// <summary>
        /// Esegue l'sfx prelevandolo dalla cartella Resource di Unity. Assicurarsi che sia quello il nome!
        /// </summary>
        public void PlaySfxByIdFromResource(string id) {
            AudioEmitter tmp = FindFreeSFXEmitter();
            tmp.Source.clip = Resources.Load<AudioClip>(id);
            tmp.Source.Play();
        }

        /// <summary>
        /// Esegue l'sfx prelevandolo dalla cartella Resource di Unity. Assicurarsi che sia quello il nome!
        /// </summary>
        public void PlaySfxByIdFromResource(string id, float volume) {
            AudioEmitter tmp = FindFreeSFXEmitter();
            tmp.Source.clip = Resources.Load<AudioClip>(id);
            tmp.Source.volume = volume;
            tmp.Source.Play();
        }

        /// <summary>
        /// Esegue l'sfx prelevandolo dalla cartella Resource di Unity. Assicurarsi che sia quello il nome!
        /// </summary>
        public void PlaySfxByIdFromResource(string id, float volume, float pitch) {
            AudioEmitter tmp = FindFreeSFXEmitter();
            tmp.Source.clip = Resources.Load<AudioClip>(id);
            tmp.Source.volume = volume;
            tmp.Source.pitch = pitch;
            tmp.Source.Play();
        }

        /// <summary>
        /// Esegue la traccia musicale dopo delay secondi
        /// </summary>
        /// <param name="id">Ordine nel vettore. Es. 0 farà eseguire la prima traccia, 1 la seconda, etc.</param>
        /// <param name="delay">Secondi da aspettare.</param>
        public void PlayMusicById(short id, float delay) {
            StartCoroutine(PlayTrackDelayed(delay, musicClips[id]));
        }

        /// <summary>
        /// Esegue la traccia musicale dopo delay secondi
        /// </summary>
        /// <param name="id">Nome della traccia.</param>
        /// <param name="delay">Secondi da aspettare</param>
        public void PlayMusicById(string id, float delay) {
            for (int i = 0; i < musicClips.Length; i++) {
                if (musicClips[i].name == id) {
                    StartCoroutine(PlayTrackDelayed(delay, musicClips[i]));
                    break;
                }
            }
        }

        /// <summary>
        /// Ferma la musica.
        /// </summary>
        public void StopMusic() {
            getMusicEmitter().Source.Stop();
        }

        /// <summary>
        /// Ferma la musica gradualmente.
        /// </summary>
        /// <param name="Time">Quanti secondi ci vogliono per portare la musica a 0</param>
        public void FadeMusicOut(float Time) {
            StartCoroutine(CoroutineFadeMusicOut(Time * 2)); //*2 perché nella coroutine invece di aspettare 1 secondo ne aspetta mezzo. In questo modo è più progressivo l'effetto fade.
        }

        /// <summary>
        /// Setta il volume di tutti gli emitters SFX
        /// </summary>
        /// <param name="Volume">Da 0.0f a 1.0f</param>
        public void SetSFXsVolume(float Volume) {
            for (int i = 0; i < emittersList.Count; i++) {
                if (emittersList[i].Type == EmitterType.sfx)
                    emittersList[i].Source.volume = Volume;
            }
        }

        /// <summary>
        /// Setta il volume dell'emitter della musica
        /// </summary>
        /// <param name="Volume">Da 0.0f a 1.0f</param>
        public void SetMusicsVolume(float Volume) {
            for (int i=0; i< emittersList.Count; i++)
                if (emittersList[i].Type == EmitterType.music)
                    emittersList[i].Source.volume = Volume;
        }

        /// <summary>
        /// Esegue l'SFX nel vettore degli SFX statici. Nel caso stia eseguendo il precedente, lo ferma e poi lo riesegue da capo.
        /// </summary>
        /// <param name="Order">L'ordine dell'SFX nel vettore</param>
        /// <param name="Volume">Il volume dell'SFX</param>
        /// <param name="Pitch">Il Pitch dell'SFX</param>
        public void PlayStaticSFX(int Order, float Volume, float Pitch) {
            staticEmittersList[Order].Source.Stop();
            staticEmittersList[Order].Source.volume = Volume;
            staticEmittersList[Order].Source.pitch = Pitch;
            staticEmittersList[Order].Source.Play();
        }

        //-------------------------------------------------------------------------------------------------------------------- FUNZIONI DI UNITY
        private void Awake() {

            //Controllo singleton
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this.gameObject);
            }

            //Controllo don't destroy on load
            if (dontDestroyOnLoad) {
                DontDestroyOnLoad(this.gameObject);
            }

            //Controllo audiosource
            for (int k = 0; k < this.gameObject.GetComponents<AudioSource>().Length; k++) {
                Destroy(this.gameObject.GetComponents<AudioSource>()[0]);
            }
            bool has_Music = false, has_SFX = false;
            int i = 0;
            if (musicClips.Length > 0) {
                has_Music = true;
                i++;
            }
            try {
                if (sfxClips.Length > 0) {
                    has_SFX = true;
                    i++;
                }
            } catch (System.Exception e) {
                Debug.LogWarning("-AudioManager- Nessun SFX! ["+e+"]");

            }
            if (staticSfxClips.Length > 0) {
                staticEmittersList = new List<AudioEmitter>(staticSfxClips.Length);
                for (i = 0; i < staticEmittersList.Capacity; i++) {
                    staticEmittersList.Add(new AudioEmitter { Type = EmitterType.staticsfx, Source = this.gameObject.AddComponent<AudioSource>() });
                    staticEmittersList[i].Source.clip = staticSfxClips[i];
                }
            }

            //audioEmitters = new AudioEmitter[i];
            emittersList = new List<AudioEmitter>(5);
            for (i = i - 1; i >= 0; i--) {
                if (has_Music) {
                    has_Music = false;
                    emittersList.Add(new AudioEmitter { Type = EmitterType.music, Source = this.gameObject.AddComponent<AudioSource>() });
                    emittersList[emittersList.Count - 1].Source.volume = 1.0f;
                } else if (has_SFX) {
                    has_SFX = false;
                    emittersList.Add(new AudioEmitter { Type = EmitterType.sfx, Source = this.gameObject.AddComponent<AudioSource>() });
                }
            }

            //Controllo audiolistener
            if (!this.gameObject.GetComponent<AudioListener>() && autoCreateListener) {
                this.gameObject.AddComponent<AudioListener>();
            }

            if (autoPlayMusic) {
                if (musicClips.Length > 0)
                    StartCoroutine(MusicPlayRandomEndless());
            }
        }
    }
}