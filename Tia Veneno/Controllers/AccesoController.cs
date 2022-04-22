using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Tia_Veneno.Models;
using System.Data.SqlClient;
using System.Data;

namespace Tia_Veneno.Controllers
{
    public class AccesoController : Controller
    {

        static string cadena = "Data Source=(local);Initial Catalog=DB_proyecto;User ID=sa;Password=123456;Integrated Security=true;";

        // GET: Acceso
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registrar(Usuario ousuario)
        {
            bool registrado;
            string mensaje;

            if (ousuario.clave == ousuario.confirmarclave)
            {
                ousuario.clave = ConvertirPassword(ousuario.clave);
            }
            else {
                ViewData["Mensaje"] = "Las Contraseñas no coinciden";

                return View();
            }

            using (SqlConnection cnn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", cnn);
                cmd.Parameters.AddWithValue("@correo", ousuario.correo);
                cmd.Parameters.AddWithValue("@password", ousuario.clave);
                cmd.Parameters.AddWithValue("@Registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@mensaje", SqlDbType.VarChar).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;
                cnn.Open();
                cmd.ExecuteNonQuery();

                registrado = Convert.ToBoolean(cmd.Parameters["registrado"].Value);
                mensaje = cmd.Parameters["mensaje"].Value.ToString();

            }
            ViewData["mensaje"] = mensaje;
            if (registrado)
            {
                return RedirectToAction("Login", "Acceso");

            }
            else {
                return View();
            }


        }
        [HttpPost]
        public ActionResult Login(Usuario ousuario)
        {

            try
            {
                ousuario.clave = ConvertirPassword(ousuario.clave);
                using (SqlConnection cnn = new SqlConnection(cadena))
                {
                    SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", cnn);
                    cmd.Parameters.AddWithValue("Correo", ousuario.correo);
                    cmd.Parameters.AddWithValue("Password", ousuario.clave);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cnn.Open();
                    ousuario.IdUsuario = Convert.ToInt32(cmd.ExecuteScalar().ToString());


                }
                if (ousuario.IdUsuario != 0)
                {
                    Session["Usuario"] = ousuario;
                    return RedirectToAction("index", "Home");
                }
                else
                {
                    ViewData["Mensaje"] = "Usuario no encontrado";
                    return View();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }
    

        public static string ConvertirPassword(string texto)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));
                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}