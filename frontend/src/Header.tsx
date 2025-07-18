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
		<header className="w-full bg-white shadow-md py-4 px-16 flex justify-between items-centers">
			{/* Logo Section */}
			<Link to="/">
				<div className="flex items-center gap-2">
					<h1 className="text-2xl font-bold text-blue-600">Edge Stats</h1>
				</div>
			</Link>

			<div className="flex items-center space-x-4">
				<p className="text-sm text-gray-500">
					Last updated: <span id="last-updated">2 mins ago</span>
				</p>
				<button className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">Refresh Odds</button>
			</div>

			{/* Authentication Section */}
			{/* <div>
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
							<button className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition">Sign Up</button>
						</Link>
						<Link to="/login">
							<button className="px-4 py-2 bg-gray-300 text-gray-800 rounded-lg hover:bg-gray-400 transition">Login</button>
						</Link>
					</div>
				)}
			</div> */}
		</header>
	);
}

export default Header;
