import { Link } from "react-router-dom";
import { FaUserCircle } from "react-icons/fa";
import AuthContext from "./context/AuthContext";
import { useContext } from "react";

function Header() {
  const authContext = useContext(AuthContext);

  if (!authContext) {
    throw new Error("AuthContext must be used within an AuthProvider");
  }

  const { user, logoutUser } = authContext;

  return (
    <header className="w-full bg-white shadow-md py-4 px-16 flex justify-between items-center fixed">
      {/* Logo Section */}
      <Link to="/">
        <div className="flex items-center gap-2">
          <img src="/logos/react.svg" alt="React Logo" className="h-12 w-12 object-contain" />
          <span className="text-3xl font-bold text-gray-700">+</span>
          <img src="/logos/django.svg" alt="Django Logo" className="h-12 w-12 object-contain" />
        </div>
      </Link>

      {/* Authentication Section */}
      <div>
        {user ? (
          <div className="flex items-center gap-4">
            <FaUserCircle className="text-gray-700 text-2xl" />
            <button
              onClick={logoutUser} // Call the logout function here
              className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition"
            >
              Logout
            </button>
          </div>
        ) : (
          <div className="flex gap-4 items-center">
            <Link to="/signup">
              <button className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition">
                Sign Up
              </button>
            </Link>
            <Link to="/login">
              <button className="px-4 py-2 bg-gray-300 text-gray-800 rounded-lg hover:bg-gray-400 transition">
                Login
              </button>
            </Link>
          </div>
        )}
      </div>
    </header>
  );
}

export default Header;
