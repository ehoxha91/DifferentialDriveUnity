using UnityEngine;
using UnityEngine.UI;

public class TankMovement : MonoBehaviour
{
    public Camera m_Camera;
    public AudioSource m_MovementAudio;    
    public AudioClip m_EngineIdling;       
    public AudioClip m_EngineDriving;      
    public float m_PitchRange = 0.2f;

    public float B = 20f;
    public float R1 = 10f;
    public float R2 = 10f;
   
    private float r1;
    private float r2;
    private float x, y;
    private float theta = Mathf.PI/2;

    private float V;
    private float W;

   
    private Rigidbody m_Rigidbody;       
    private float m_OriginalPitch;

    public Text txtPose;
    public Text txtTrajectory;
    public Text txtRightWheelV;
    public Text txtLeftWheelV;
    public Text txtUpdateRate;
    public Text txtTimeElapsed;

    private bool camView = false;
    

    private float timeoffset = 0;
    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        //we store component reference to this variable
        r1 = R1 / 2f;
        r2 = R2 / 2f;
        txtTrajectory.text = "Trajectory: A";
        txtRightWheelV.text = "Wr = 5*sin(3t)";
        txtLeftWheelV.text = "Wl = 5*sin(3t)";
    }


    private void OnEnable ()
    {
        //before any update happens
        m_Rigidbody.isKinematic = true; 
    }


    private void OnDisable ()
    {
        m_Rigidbody.isKinematic = true; //I need to use this one like this
                                        // just position control;
                                        // No forces, just position and velocity
    }


    private void Start()
    {
        m_OriginalPitch = m_MovementAudio.pitch;
    }

    private bool mode = false; // true = B; false = A;
    private void Update()
    {
        bool restart_ = Input.GetKeyDown(KeyCode.R);
        bool changeModeA = Input.GetKeyDown(KeyCode.A);
        bool changeModeB = Input.GetKeyDown(KeyCode.B);
        bool toggleCam = Input.GetKeyDown(KeyCode.C);

        if(restart_)
        {
            RestartMovement();
        }

        if(changeModeA)
        {
            mode = false;
            RestartMovement();
            txtTrajectory.text = "Trajectory: A";
            txtRightWheelV.text = "Wr = 5*sin(3t)";
            txtLeftWheelV.text = "Wl = 5*sin(3t)";
        }
        if (changeModeB)
        {
            mode = true;
            RestartMovement();
            txtTrajectory.text = "Trajectory: B";
            txtRightWheelV.text = "Wr = 5*sin(3t+1)";
            txtLeftWheelV.text = "Wl = 5*sin(3t)";
        }

        if(toggleCam)
        {
            camView = !camView; //toggle cammera
            ToggleCameraView();
        }
            
        
        //EngineAudio(); //Play the sounds
    }

    private void ToggleCameraView()
    {
        if(camView ==false)
        {
            Vector3 camposition = new Vector3(-6.5f, 55.6f, -24.13f);
            Quaternion camRot = Quaternion.Euler(70f, 0f, 0f);
            m_Camera.transform.position = camposition;
            m_Camera.transform.rotation = camRot;
        }
        else
        {
            Vector3 camposition = new Vector3(32.6f, 55.6f, -35.3f);
            Quaternion camRot = Quaternion.Euler(46.51f, -62.58f, 5.3f);
            m_Camera.transform.position = camposition;
            m_Camera.transform.rotation = camRot;
        }
    }
    
    private void RestartMovement()
    {
        //put everything to the start position;
        Quaternion rotate = Quaternion.Euler(0f, 0f, 0f);
        m_Rigidbody.MoveRotation(rotate);
        Vector3 movement = new Vector3(26f, 0f, -20f);
        m_Rigidbody.MovePosition(movement);
        x = 0;
        y = 0;
        theta = Mathf.PI / 2;
        timeoffset = Time.time;
        
    }

    private void EngineAudio()
    {
        if((Time.time-timeoffset)>20)
        {
            if(m_MovementAudio.clip == m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineIdling; //Change sound to idle engine;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving; //Change sound to idle engine;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }

    private void FixedUpdate()
    {
        if((Time.time - timeoffset)<= 20f)
        {
            // Move and turn the tank.  
            DifferentialDrive();
            Move(); //Do the moving;
            Turn(); //Do the turning;
            txtTimeElapsed.text = "Time Elapsed: " + (Time.time - timeoffset) ;
        }
        txtUpdateRate.text = "dt = "+ Time.deltaTime.ToString();

    }

    private float lastTime;
    private void DifferentialDrive()
    {
        float _vr = 0;
        float _vl = 0;
        if (mode)
        {
            _vr = r1 * 4 * Mathf.Sin(3 * (Time.time - timeoffset)+1);
            _vl = r2 * 5 * Mathf.Sin(3 * (Time.time - timeoffset));
        }
        else
        {
            _vr = r1 * 5 * Mathf.Sin(3 * (Time.time - timeoffset));
            _vl = r2 * 5 * Mathf.Sin(3 * (Time.time - timeoffset));
        }

        V = (_vr + _vl) / 2f;
        W = (_vr - _vl) / B;
        
        float _theta_change = W * (0.001f);
        if(_vl == _vr)
        {
            x = x + _vr * (Time.time - lastTime) * Mathf.Cos(theta);
            y = y + _vr * (Time.time - lastTime) * Mathf.Sin(theta);
            theta = theta + _theta_change;
        }
        else
        {
            //float R = (B / 2f) * ((_vr + _vl) / (_vr - _vl));
            x = x - (V / W) * Mathf.Sin(theta) + (V / W) * Mathf.Sin(theta + _theta_change);
            y = y + (V / W) * Mathf.Cos(theta) - (V / W) * Mathf.Cos(theta + _theta_change);
            theta = theta + _theta_change;
        }
        lastTime = Time.time;

    }

    private void Move()
    {
        Vector3 movement = new Vector3((x) + 26, 0f, (y) - 20);//new Vector3((-y)+26, 0f, (x)-20);
        m_Rigidbody.MovePosition(movement);
    }


    private void Turn()
    {
        float theta_deg = - Mathf.Rad2Deg*theta + 90f;
        Quaternion rotate = Quaternion.Euler(0f, theta_deg, 0f);
        m_Rigidbody.MoveRotation(rotate);
        float thdeg = Mathf.Rad2Deg * theta;
        string txt = "[x y theta] = [" + x.ToString() + "  " + y.ToString() + " " + thdeg.ToString() + "]";
        txtPose.text = txt;
    }
}