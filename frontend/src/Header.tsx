import { Link } from "react-router-dom";
import AuthContext from "./context/AuthContext";
import { useContext, useEffect, useRef, useState } from "react";
import { FaFilter } from "react-icons/fa";
import axios from "axios";

function Header() {
	const [showFilters, setShowFilters] = useState(false);
	const [selectedLeagues, setSelectedLeagues] = useState<string[]>([]);
	const [selectedBooks, setSelectedBooks] = useState<string[]>([]);
	const filterRef = useRef<HTMLDivElement>(null);

	const handleSportsChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
		const values = Array.from(e.target.selectedOptions, (option) => option.value);
		setSelectedLeagues(values);
	};

	const handleBooksChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
		const values = Array.from(e.target.selectedOptions, (option) => option.value);
		setSelectedBooks(values);
	};

	const handleRefreshOdds = async () => {
		try {
			const response = await axios.post("https://localhost:7105/scraper", {
				Leagues: selectedLeagues,
				Sportsbooks: selectedBooks,
			});

			const { recordsSaved, sourceCount, sportCount } = response.data;

			console.log(`Scraped ${recordsSaved} odds from ${sourceCount} sportsbooks across ${sportCount} sports.`);
		} catch (error) {
			console.error("Error scraping odds:", error);
		}
	};

	// Click outside handler
	useEffect(() => {
		function handleClickOutside(event: MouseEvent) {
			if (filterRef.current && !filterRef.current.contains(event.target as Node)) {
				setShowFilters(false);
			}
		}

		document.addEventListener("mousedown", handleClickOutside);
		return () => {
			document.removeEventListener("mousedown", handleClickOutside);
		};
	}, [filterRef]);

	const authContext = useContext(AuthContext);

	if (!authContext) {
		throw new Error("AuthContext must be used within an AuthProvider");
	}

	  const { user, logoutUser } = authContext;

	return (
		<header className="w-full bg-sea_green shadow-md py-4 px-8 flex justify-between items-centers">
			{/* Logo Section */}
			<Link to="/">
				<div className="flex items-center gap-2">
					<h1 className="text-2xl font-bold text-blue-600">Edge Stats</h1>
				</div>
			</Link>
			<div className="flex gap-4">
				<div className="flex items-center gap-4">
					<p className="text-sm text-gray-500">
						Last updated: <span id="last-updated">2 mins ago</span>
					</p>
					<button className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700" onClick={handleRefreshOdds}>
						Refresh Odds
					</button>
				</div>

				{/* Filters Icon Button */}
				<div className="flex gap-4 relative" ref={filterRef}>
					<button onClick={() => setShowFilters(!showFilters)} className="text-gray-600 hover:text-blue-600 transition" title="Filters">
						<FaFilter className="w-5 h-5" />
					</button>

					{/* Dropdown Menu */}
					{showFilters && (
						<div className="absolute right-0 top-12 mt-2 w-64 bg-white border rounded shadow-md p-4 z-50">
							<h3 className="text-sm font-semibold text-gray-700 mb-3">Filters</h3>

							<div className="flex flex-col gap-4">
								{/* Sports Multiselect */}
								<div>
									<label className="block text-sm text-gray-600 mb-1">Sports</label>
									<select
										multiple
										className="w-full h-24 border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
										value={selectedLeagues}
										onChange={handleSportsChange}
									>
										<option value="nfl">NFL</option>
									</select>
								</div>

								{/* Sportsbook Multiselect */}
								<div>
									<label className="block text-sm text-gray-600 mb-1">Sportsbooks</label>
									<select
										multiple
										className="w-full h-24 border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
										value={selectedBooks}
										onChange={handleBooksChange}
									>
										<option value="Pinnacle">Pinnacle</option>
									</select>
								</div>
							</div>
						</div>
					)}
				</div>
			</div>

			{/* Authentication Section */}
			<div>
				{user ? (
					<div className="flex items-center gap-4">
						{/* <FaUserCircle className="text-gray-700 text-2xl" /> */}
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
			</div>
		</header>
	);
}

export default Header;
