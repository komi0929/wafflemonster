using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// モンスターの視覚的アニメーション
    /// 浮遊ボブ、目のプレイヤー追従、状態変化時のフィードバック
    /// </summary>
    public class MonsterVisualAnimator : MonoBehaviour
    {
        [Header("浮遊")]
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobFrequency = 1.5f;
        [SerializeField] private float _bobOffset;

        [Header("回転")]
        [SerializeField] private float _idleRotateSpeed = 15f;

        private Transform[] _eyes;
        private Transform _player;
        private Vector3 _basePosition;
        private MonsterBase _monster;
        private GameObject _alertMark;
        private GameObject _pakuText;
        private float _alertTimer;
        private float _pakuTimer;

        private void Start()
        {
            _basePosition = transform.position;
            _monster = GetComponent<MonsterBase>();
            _bobOffset = Random.value * Mathf.PI * 2f;

            // 目を探す
            var eyeList = new System.Collections.Generic.List<Transform>();
            FindChildrenByName(transform, "Eye_Pupil", eyeList);
            _eyes = eyeList.ToArray();

            // プレイヤー検索
            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;

            // アラートマーク（「！」）
            CreateAlertMark();
            CreatePakuText();
        }

        private void Update()
        {
            // 浮遊ボブ
            Vector3 pos = transform.position;
            float bob = Mathf.Sin((Time.time + _bobOffset) * _bobFrequency * Mathf.PI * 2f) * _bobAmplitude;
            transform.position = new Vector3(pos.x, _basePosition.y + bob, pos.z);
            _basePosition = new Vector3(pos.x, _basePosition.y, pos.z);

            // アイドル回転
            if (_monster != null && _monster.CurrentState == MonsterBase.MonsterAIState.Patrol)
            {
                transform.Rotate(0, _idleRotateSpeed * Time.deltaTime, 0);
            }

            // 目のプレイヤー追従
            if (_player != null && _eyes.Length > 0)
            {
                foreach (var eye in _eyes)
                {
                    if (eye == null) continue;
                    Vector3 lookDir = (_player.position - eye.position).normalized;
                    Vector3 localLook = eye.parent.InverseTransformDirection(lookDir);
                    // 視線の制限（目が飛び出さないように）
                    localLook = Vector3.ClampMagnitude(localLook, 0.3f);
                    eye.localPosition = new Vector3(localLook.x, localLook.y, 0.35f);
                }
            }

            // アラートマーク管理
            if (_alertMark != null)
            {
                if (_alertTimer > 0)
                {
                    _alertTimer -= Time.deltaTime;
                    _alertMark.SetActive(true);
                    // パルス
                    float scale = 1f + Mathf.Sin(Time.time * 10f) * 0.2f;
                    _alertMark.transform.localScale = Vector3.one * scale * 0.5f;
                    // カメラに向ける
                    if (Camera.main != null)
                        _alertMark.transform.LookAt(Camera.main.transform);
                }
                else
                {
                    _alertMark.SetActive(false);
                }
            }

            // PakuText管理
            if (_pakuText != null)
            {
                if (_pakuTimer > 0)
                {
                    _pakuTimer -= Time.deltaTime;
                    _pakuText.SetActive(true);
                    _pakuText.transform.localPosition = new Vector3(0, 2f + (1f - _pakuTimer) * 0.5f, 0);
                    if (Camera.main != null)
                        _pakuText.transform.LookAt(Camera.main.transform);
                }
                else
                {
                    _pakuText.SetActive(false);
                }
            }
        }

        /// <summary>
        /// モンスターが「！」を表示（検知時）
        /// </summary>
        public void ShowAlert(float duration = 1f)
        {
            _alertTimer = duration;
        }

        /// <summary>
        /// 「PAKU!」テキストを表示（食事時）
        /// </summary>
        public void ShowPaku(float duration = 1.5f)
        {
            _pakuTimer = duration;
        }

        private void CreateAlertMark()
        {
            _alertMark = new GameObject("AlertMark");
            _alertMark.transform.SetParent(transform);
            _alertMark.transform.localPosition = new Vector3(0, 2.5f, 0);

            // 「！」を球体で表現
            var excl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            excl.transform.SetParent(_alertMark.transform);
            excl.transform.localPosition = Vector3.zero;
            excl.transform.localScale = new Vector3(0.3f, 0.5f, 0.1f);
            Destroy(excl.GetComponent<SphereCollider>());

            var mat = excl.GetComponent<Renderer>().material;
            mat.color = Color.red;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.red * 3f);

            _alertMark.SetActive(false);
        }

        private void CreatePakuText()
        {
            _pakuText = new GameObject("PakuText");
            _pakuText.transform.SetParent(transform);
            _pakuText.transform.localPosition = new Vector3(0, 2f, 0);

            // 「PAKU!」を小球群で表現
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(_pakuText.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = new Vector3(0.6f, 0.25f, 0.1f);
            Destroy(sphere.GetComponent<SphereCollider>());

            var mat = sphere.GetComponent<Renderer>().material;
            mat.color = new Color(1f, 0.85f, 0.4f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.4f) * 3f);

            _pakuText.SetActive(false);
        }

        private void FindChildrenByName(Transform parent, string name, System.Collections.Generic.List<Transform> results)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    results.Add(child);
                FindChildrenByName(child, name, results);
            }
        }
    }
}
