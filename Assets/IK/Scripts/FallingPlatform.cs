using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public enum Axis
    {
        x,
        y,
        z
    }

    public float requiredStandTime = 2;
    public float shakeSpeed = 50;
    public float shakeAmount = 0.25f;
    public Axis shakeAxis;
    private readonly ObservableList<Collider> _enemiesParts = new ObservableList<Collider>();
    private Vector3 _originalPosition;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _enemiesParts.CountChanged += OnEnemiesPartsCountChanged;
        _originalPosition = transform.position;
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var shake = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        var newPosition = _originalPosition;
        switch (shakeAxis)
        {
            case Axis.x:
                newPosition.x += shake;
                break;
            case Axis.y:
                newPosition.y += shake;
                break;
            case Axis.z:
                newPosition.z += shake;
                break;
        }

        transform.position = newPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
            if (other.transform.root.GetComponent<HealthManager>())
                if (!_enemiesParts.Contains(other))
                    _enemiesParts.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
            if (other.transform.root.GetComponent<HealthManager>())
                _enemiesParts.Remove(other);
    }

    private void OnEnemiesPartsCountChanged(int count)
    {
        switch (count)
        {
            case 0:
                enabled = false;
                StopAllCoroutines();
                break;
            case 1:
                enabled = true;
                StartCoroutine(Fall());
                break;
        }
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(requiredStandTime);
        _rigidbody.isKinematic = false;
        Destroy(this);
    }
}