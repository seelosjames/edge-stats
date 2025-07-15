import { createContext, useState, useEffect, ReactNode } from "react";
import { useNavigate } from "react-router-dom";

interface AuthTokens {
  access: string;
  refresh: string;
}

interface AuthContextType {
  user: string | null;
  authTokens: AuthTokens | null;
  loginUser: (email: string, password: string) => Promise<void>;
  registerUser: (email: string, password: string) => Promise<void>;
  logoutUser: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<string | null>(null);
  const [authTokens, setAuthTokens] = useState<AuthTokens | null>(() =>
    localStorage.getItem("authTokens") ? JSON.parse(localStorage.getItem("authTokens")!) : null
  );
  const navigate = useNavigate();

  const loginUser = async (email: string, password: string) => {
    const response = await fetch("http://127.0.0.1:8000/authentication/token/", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username: email, password }),
    });

    if (response.ok) {
      const data = await response.json();
      setAuthTokens(data); // Set the tokens to state
      setUser(email); // Set the user
      localStorage.setItem("authTokens", JSON.stringify(data)); // Store tokens in localStorage
      navigate("/"); // Redirect to the home page (or the appropriate page)
    } else {
      alert("Login failed");
    }
  };


  const registerUser = async (email: string, password: string) => {
    const response = await fetch("http://127.0.0.1:8000/authentication/register/", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });

    if (response.ok) {
      console.log(response);
      // await loginUser(email, password);
    } else {
      alert("Registration failed");
    }
  };

  const logoutUser = () => {
    setAuthTokens(null);
    setUser(null);
    localStorage.removeItem("authTokens");
    navigate("/login");
  };

  useEffect(() => {
    const refreshToken = async () => {
      if (!authTokens) return;

      const response = await fetch("http://127.0.0.1:8000/authentication/token/refresh/", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refresh: authTokens.refresh }),
      });

      if (response.ok) {
        const data: AuthTokens = await response.json();
        setAuthTokens(data);
        localStorage.setItem("authTokens", JSON.stringify(data));
      } else {
        logoutUser();
      }
    };

    const interval = setInterval(refreshToken, 1000 * 60 * 4);
    return () => clearInterval(interval);
  }, [authTokens]);

  return (
    <AuthContext.Provider value={{ user, authTokens, loginUser, registerUser, logoutUser }}>
      {children}
    </AuthContext.Provider>
  );
};

export default AuthContext;
