-- Exchange Strength is constant
function GetJValueByLatticeIndex(x, y)
    return 2.0
end

-- Need to register the function
return { 
    GetJValueByLatticeIndex = GetJValueByLatticeIndex,
}