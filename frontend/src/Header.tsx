import { Link } from "react-router-dom";
import AuthContext from "./context/AuthContext";
import { useContext, useEffect, useRef, useState } from "react";
import { FaFilter } from "react-icons/fa";

function Header() {
	const authContext = useContext(AuthContext);

	if (!authContext) {
		throw new Error("AuthContext must be used within an AuthProvider");
	}

	const { user, logoutUser } = authContext;

	return (
		<header className="bg-seagreen py-3 px-12 flex justify-between items-center shadow-xl relative">
			{/* Logo Section */}
			<div className="flex items-center">
				<Link to="/">
					<div className="flex items-center gap-2 h-full">
						<h1 className="text-2xl font-bold text-white">Edge Stats</h1>
					</div>
				</Link>
			</div>

			{/* Authentication Section */}
			<div>
				{user ? (
					<div className="flex items-center">
						<button onClick={logoutUser} className="px-4 bg-red-500 text-white rounded-lg hover:bg-red-600 transition">
							Logout
						</button>
					</div>
				) : (
					<div className="flex gap-4 items-center">
						<Link to="/signup">
							<button className="px-4 py-1 bg-white text-text border border-seagreen rounded-lg hover:bg-gray-100 transition">
								Sign Up
							</button>
						</Link>
						<Link to="/login">
							<button className="px-4 py-1 bg-seagreen text-white border border-white rounded-lg hover:bg-seagreen-400 transition">
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

